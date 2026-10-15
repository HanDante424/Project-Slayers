using System;
using System.Collections.Generic;
using UnityEngine;

public partial class MayhemMechanism
{
    [Serializable]
    private sealed class SpearMotion
    {
        public AnimationClip animationClip;
        public string animatorState;
        public string animatorTrigger;
        public SpearMotion(string suffix)
        {
            animatorState = "Playerable_Mayhem_Skill_Motion_011_BloodSpear_" + suffix;
            animatorTrigger = "BloodSpear" + suffix;
        }
    }

    [Serializable]
    private sealed class SpearEffect
    {
        public GameObject effectPrefab;
        public AnimationClip effectClip;
        public Vector2 spawnOffset;
        public EffectAttackSettings attackSettings = new();
    }

    private enum SpearPhase { Inactive, WaitingReady, Ready, WaitingDash, Dash, WaitingFinish, Finish }
    private static readonly int SpearPlayingHash = Animator.StringToHash("BloodSpearPlaying");

    [Header("Blood Spear Motion")]
    [SerializeField] private SpearMotion bloodSpearReady = new("Ready");
    [SerializeField] private SpearMotion bloodSpearAttackDash = new("AttackDash");
    [SerializeField] private SpearMotion bloodSpearAttackFinish = new("AttackFinish");

    [Header("Blood Spear Dash")]
    [Min(0)] [SerializeField] private float bloodSpearDashDistance = 5f;
    [Min(0)] [SerializeField] private float bloodSpearDashSpeed = 2.5f;
    [Tooltip("Safety timeout when terrain prevents reaching the distance; does not emit Effect 01.")]
    [Min(0)] [SerializeField] private float bloodSpearMaximumDashDuration = 5f;

    [Header("Blood Spear Effect 00")]
    [SerializeField] private SpearEffect bloodSpearEffect00 = new();
    [Header("Blood Spear Effect 01 (Temporary Test Only)")]
    [SerializeField] private SpearEffect bloodSpearEffect01 = new();
    [Tooltip("TEMP: emit only on maximum distance reached. No hit detection, resource use or damage.")]
    [SerializeField] private bool enableTemporaryBloodSpearFollowUpEffect = true;

    private readonly List<GameObject> spearEffects = new();
    private SpearPhase spearPhase;
    private float spearDashStartedAt;
    private float spearDashStartX;
    private float spearFacing;
    private bool spearOwnsMovement;
    private bool spearOwnsSkillVelocity;
    private CharacterMotor2D.MovementControl spearPreviousMovement;

    private bool IsSpearActing => spearPhase != SpearPhase.Inactive;

    private bool TryStartBloodSpear()
    {
        if (!isActiveAndEnabled || IsActing || motor == null || !motor.IsGrounded || motor.IsHitStunLocked)
            return false;

        if (!HasSpearReferences())
        {
            Debug.LogWarning("Blood Spear requires its motion/effect references and positive dash speed/duration.", this);
            return false;
        }

        spearPreviousMovement = motor.CurrentMovementControl;
        spearOwnsMovement = true;
        motor.SetMovementControl(CharacterMotor2D.MovementControl.Locked);
        spearPhase = SpearPhase.WaitingReady;
        animator.SetBool(SpearPlayingHash, true);
        animator.SetTrigger(bloodSpearReady.animatorTrigger);
        return true;
    }

    private bool HasSpearReferences()
    {
        return animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null &&
            effectSpawnPoint != null && bloodSpearDashSpeed > 0f && bloodSpearMaximumDashDuration > 0f &&
            animator.GetBehaviours<BloodSpearMotionRelay>().Length > 0 &&
            HasSpearMotion(bloodSpearReady) && HasSpearMotion(bloodSpearAttackDash) && HasSpearMotion(bloodSpearAttackFinish) &&
            HasSpearEffect(bloodSpearEffect00) && (!enableTemporaryBloodSpearFollowUpEffect || HasSpearEffect(bloodSpearEffect01));
    }

    private bool HasSpearMotion(SpearMotion motion)
    {
        return motion.animationClip != null && motion.animationClip.length > 0f &&
            !string.IsNullOrEmpty(motion.animatorTrigger) &&
            animator.HasState(0, SpearStateHash(motion));
    }

    private static bool HasSpearEffect(SpearEffect effect)
    {
        return effect.effectPrefab != null && effect.effectClip != null && effect.effectClip.length > 0f;
    }

    private static int SpearStateHash(SpearMotion motion) => Animator.StringToHash("Base Layer." + motion.animatorState);

    public void NotifyBloodSpearMotionEntered(int stateHash)
    {
        if (!IsSpearActing)
            return;
        if (spearPhase == SpearPhase.WaitingReady && stateHash == SpearStateHash(bloodSpearReady))
            spearPhase = SpearPhase.Ready;
        else if (spearPhase == SpearPhase.WaitingDash && stateHash == SpearStateHash(bloodSpearAttackDash))
        {
            spearPhase = SpearPhase.Dash;
            spearFacing = motor.transform.localScale.x < 0f ? -1f : 1f;
            spearDashStartX = motor.PhysicsPosition.x;
            spearDashStartedAt = Time.time;
            spearOwnsSkillVelocity = true;
            motor.SetSkillDashVelocity(spearFacing * bloodSpearDashSpeed, spearDashStartX + spearFacing * bloodSpearDashDistance);
            SpawnSpearEffect(bloodSpearEffect00, effectSpawnPoint.position, followPlayer: true);
        }
        else if (spearPhase == SpearPhase.WaitingFinish && stateHash == SpearStateHash(bloodSpearAttackFinish))
            spearPhase = SpearPhase.Finish;
    }

    public void NotifyBloodSpearMotionUpdated(int stateHash, float normalizedTime)
    {
        if (spearPhase == SpearPhase.Ready && stateHash == SpearStateHash(bloodSpearReady) && normalizedTime >= 1f)
        {
            spearPhase = SpearPhase.WaitingDash;
            animator.SetTrigger(bloodSpearAttackDash.animatorTrigger);
        }
    }

    public void NotifyBloodSpearMotionExited(int stateHash, bool completed)
    {
        if (!IsSpearActing)
            return;
        if (stateHash == SpearStateHash(bloodSpearReady) && spearPhase == SpearPhase.WaitingDash)
            return;
        if (stateHash == SpearStateHash(bloodSpearAttackDash) && spearPhase == SpearPhase.WaitingFinish)
            return;
        if (stateHash == SpearStateHash(bloodSpearAttackFinish) && spearPhase == SpearPhase.Finish && completed)
            CompleteBloodSpear();
        else
            CancelBloodSpear();
    }

    private void UpdateBloodSpear()
    {
        if (IsSpearActing && (animator == null || !animator.isActiveAndEnabled || motor == null ||
            !motor.isActiveAndEnabled || motor.IsHitStunLocked))
            CancelBloodSpear();
    }

    private void FixedUpdateBloodSpear()
    {
        if (spearPhase != SpearPhase.Dash)
            return;

        float actualDistance = (motor.PhysicsPosition.x - spearDashStartX) * spearFacing;
        if (actualDistance >= bloodSpearDashDistance - 0.0001f)
            StopBloodSpearDash(reachedDistance: true);
        else if (Time.time - spearDashStartedAt >= bloodSpearMaximumDashDuration)
            StopBloodSpearDash(reachedDistance: false);
    }

    // Future hit/collision logic can request early finish without pretending distance was reached.
    public void EndDashEarly()
    {
        if (spearPhase == SpearPhase.Dash)
            StopBloodSpearDash(reachedDistance: false);
    }

    private void StopBloodSpearDash(bool reachedDistance)
    {
        ClearSpearDashVelocity();
        spearPhase = SpearPhase.WaitingFinish;
        if (reachedDistance && enableTemporaryBloodSpearFollowUpEffect)
        {
            Vector2 position = motor.PhysicsPosition;
            SpawnSpearEffect(bloodSpearEffect01, new Vector3(position.x, position.y, motor.transform.position.z), followPlayer: false);
        }
        if (IsSpearActing)
            animator.SetTrigger(bloodSpearAttackFinish.animatorTrigger);
    }

    private void SpawnSpearEffect(SpearEffect effect, Vector3 origin, bool followPlayer)
    {
        try
        {
            spearEffects.RemoveAll(item => item == null);
            Vector3 offset = new(effect.spawnOffset.x * spearFacing, effect.spawnOffset.y, 0f);
            GameObject instance = Instantiate(effect.effectPrefab, origin + offset, effectSpawnPoint.rotation);
            spearEffects.Add(instance);
            Vector3 scale = instance.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * spearFacing;
            instance.transform.localScale = scale;
            // Sprite-only animation: keep world scale while following the dash, without editing the prefab.
            if (followPlayer)
                instance.transform.SetParent(motor.transform, worldPositionStays: true);
            InitializeMayhemEffectAttack(instance, effect.attackSettings);
            if (followPlayer)
                Destroy(instance, effect.effectClip.length);
            else
            {
                // Detached follow-up owns its playback lifetime, even if the player is hit or disabled.
                instance.AddComponent<DestroyAfterAnimatorPlayback>();
                spearEffects.Remove(instance);
            }
        }
        catch (Exception exception)
        {
            CancelBloodSpear();
            Debug.LogException(exception, this);
        }
    }

    private void CompleteBloodSpear() => CleanupBloodSpear();

    private void CancelBloodSpear()
    {
        CleanupBloodSpear();
        foreach (GameObject effect in spearEffects)
            if (effect != null) Destroy(effect);
        spearEffects.Clear();
    }

    private void ClearSpearDashVelocity()
    {
        if (spearOwnsSkillVelocity && motor != null)
            motor.ClearSkillVelocity();
        spearOwnsSkillVelocity = false;
    }

    private void CleanupBloodSpear()
    {
        ClearSpearDashVelocity();
        if (IsSpearActing && animator != null)
        {
            animator.ResetTrigger(bloodSpearReady.animatorTrigger);
            animator.ResetTrigger(bloodSpearAttackDash.animatorTrigger);
            animator.ResetTrigger(bloodSpearAttackFinish.animatorTrigger);
            animator.SetBool(SpearPlayingHash, false);
        }
        if (spearOwnsMovement && motor != null)
            motor.SetMovementControl(spearPreviousMovement);
        spearOwnsMovement = false;
        spearPhase = SpearPhase.Inactive;
    }

}
