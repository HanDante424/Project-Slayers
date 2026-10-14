using System;
using UnityEngine;

public partial class MayhemMechanism
{
    [Serializable]
    private sealed class AxeMotion
    {
        public AnimationClip animationClip;
        public string animatorState;
        public string animatorTrigger;

        public AxeMotion(string suffix)
        {
            animatorState = "Playerable_Mayhem_Skill_Motion_012_BloodAxe_" + suffix;
            animatorTrigger = "BloodAxe" + suffix;
        }
    }

    [Serializable]
    private sealed class AxeEffect
    {
        public GameObject effectPrefab;
        public AnimationClip effectAnimationClip;
        public Vector2 effectSpawnOffset;
    }

    private enum AxePhase { Inactive, WaitingReady, Ready, WaitingAttack, Attack }
    private static readonly int AxePlayingHash = Animator.StringToHash("BloodAxePlaying");

    [Header("Blood Axe Motion")]
    [SerializeField] private AxeMotion bloodAxeReady = new("Ready");
    [SerializeField] private AxeMotion bloodAxeAttack = new("Attack");
    [Header("Blood Axe Effects")]
    [SerializeField] private AxeEffect bloodAxeEffectReady = new();
    [SerializeField] private AxeEffect bloodAxeEffectAttack = new();
    [SerializeField] private AxeEffect bloodAxeEffectBurst00 = new();
    [SerializeField] private AxeEffect bloodAxeEffectBurst01 = new();

    private AxePhase axePhase;
    private CharacterMotor2D.MovementControl axePreviousMovement;
    private float axeFacing;
    private GameObject axeReadyEffect;
    private bool IsAxeActing => axePhase != AxePhase.Inactive;

    public void RequestBloodAxe()
    {
        if (!isActiveAndEnabled || IsActing || motor == null || !motor.isActiveAndEnabled ||
            !motor.IsGrounded || motor.IsHitStunLocked)
            return;
        if (!HasAxeReferences())
        {
            Debug.LogWarning("Blood Axe requires its motion, effect, Animator and spawn references.", this);
            return;
        }

        attackBuffer.Clear();
        axePreviousMovement = motor.CurrentMovementControl;
        axeFacing = motor.transform.localScale.x < 0f ? -1f : 1f;
        axePhase = AxePhase.WaitingReady;
        motor.SetMovementControl(CharacterMotor2D.MovementControl.Locked);
        animator.SetBool(AxePlayingHash, true);
        animator.SetTrigger(bloodAxeReady.animatorTrigger);
    }

    private bool HasAxeReferences() => animator != null && animator.isActiveAndEnabled &&
        animator.runtimeAnimatorController != null && effectSpawnPoint != null &&
        animator.GetBehaviours<BloodAxeMotionRelay>().Length > 0 &&
        HasAxeMotion(bloodAxeReady) && HasAxeMotion(bloodAxeAttack) &&
        HasAxeEffect(bloodAxeEffectReady) && HasAxeEffect(bloodAxeEffectAttack) &&
        HasAxeEffect(bloodAxeEffectBurst00) && HasAxeEffect(bloodAxeEffectBurst01);

    private bool HasAxeMotion(AxeMotion motion) => motion != null && motion.animationClip != null &&
        motion.animationClip.length > 0f && !string.IsNullOrEmpty(motion.animatorTrigger) &&
        !string.IsNullOrEmpty(motion.animatorState) && animator.HasState(0, AxeStateHash(motion));

    private static bool HasAxeEffect(AxeEffect effect)
    {
        if (effect == null || effect.effectPrefab == null || effect.effectAnimationClip == null ||
            effect.effectAnimationClip.length <= 0f)
            return false;
        Animator effectAnimator = effect.effectPrefab.GetComponent<Animator>();
        if (effectAnimator == null || effectAnimator.runtimeAnimatorController == null)
            return false;
        return Array.Exists(effectAnimator.runtimeAnimatorController.animationClips,
            clip => clip == effect.effectAnimationClip);
    }

    private static int AxeStateHash(AxeMotion motion) => Animator.StringToHash("Base Layer." + motion.animatorState);

    public void NotifyBloodAxeMotionEntered(int stateHash)
    {
        if (!isActiveAndEnabled || !IsAxeActing)
            return;
        try
        {
            if (axePhase == AxePhase.WaitingReady && stateHash == AxeStateHash(bloodAxeReady))
            {
                axePhase = AxePhase.Ready;
                axeReadyEffect = SpawnAxeEffect(bloodAxeEffectReady);
            }
            else if (axePhase == AxePhase.WaitingAttack && stateHash == AxeStateHash(bloodAxeAttack))
            {
                axePhase = AxePhase.Attack;
                // One presentation callback, one common origin, three independent lifetimes.
                Vector3 origin = effectSpawnPoint.position;
                SpawnAxeEffect(bloodAxeEffectAttack, origin);
                SpawnAxeEffect(bloodAxeEffectBurst00, origin);
                SpawnAxeEffect(bloodAxeEffectBurst01, origin);
            }
        }
        catch (Exception exception)
        {
            CancelBloodAxe();
            Debug.LogException(exception, this);
        }
    }

    public void NotifyBloodAxeMotionUpdated(int stateHash, float normalizedTime)
    {
        if (axePhase == AxePhase.Ready && stateHash == AxeStateHash(bloodAxeReady) && normalizedTime >= 1f)
        {
            axePhase = AxePhase.WaitingAttack;
            animator.SetTrigger(bloodAxeAttack.animatorTrigger);
        }
    }

    public void NotifyBloodAxeMotionExited(int stateHash, bool completed)
    {
        if (!IsAxeActing)
            return;
        if (stateHash == AxeStateHash(bloodAxeReady) && axePhase == AxePhase.WaitingAttack && completed)
            return;
        if (stateHash == AxeStateHash(bloodAxeAttack) && axePhase == AxePhase.Attack && completed)
            CompleteBloodAxe();
        else
            CancelBloodAxe();
    }

    private GameObject SpawnAxeEffect(AxeEffect effect) => SpawnAxeEffect(effect, effectSpawnPoint.position);

    private GameObject SpawnAxeEffect(AxeEffect effect, Vector3 origin)
    {
        Vector3 offset = new(effect.effectSpawnOffset.x * axeFacing, effect.effectSpawnOffset.y, 0f);
        GameObject instance = Instantiate(effect.effectPrefab, origin + offset, effectSpawnPoint.rotation);
        Vector3 scale = instance.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * axeFacing;
        instance.transform.localScale = scale;
        // Reuse Spear's detached one-shot lifetime; completing the short motion must not cut VFX.
        instance.AddComponent<DestroyAfterAnimatorPlayback>();
        return instance;
    }

    private void UpdateBloodAxe()
    {
        if (IsAxeActing && (animator == null || !animator.isActiveAndEnabled || motor == null ||
            !motor.isActiveAndEnabled || motor.IsHitStunLocked))
            CancelBloodAxe();
    }

    private void CompleteBloodAxe() => CleanupBloodAxe();

    private void CancelBloodAxe()
    {
        CleanupBloodAxe();
        // Startup VFX belongs to this action; detached Attack VFX behaves like Spear Effect 01.
        if (axeReadyEffect != null)
            Destroy(axeReadyEffect);
        axeReadyEffect = null;
    }

    private void CleanupBloodAxe()
    {
        if (!IsAxeActing)
            return;
        if (animator != null)
        {
            animator.ResetTrigger(bloodAxeReady.animatorTrigger);
            animator.ResetTrigger(bloodAxeAttack.animatorTrigger);
            animator.SetBool(AxePlayingHash, false);
        }
        if (motor != null)
            motor.SetMovementControl(axePreviousMovement);
        axePhase = AxePhase.Inactive;
    }
}
