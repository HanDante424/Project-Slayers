using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MayhemMechanism : MonoBehaviour, ICharacterMechanism
{
    [Serializable]
    private sealed class BloodSwordStep
    {
        public string animatorTrigger;
        public string animatorState;
        public AnimationClip animationClip;
        public GameObject effectPrefab;
        public AnimationClip effectClip;
        public Vector2 effectSpawnOffset = Vector2.zero;
        public CharacterMotor2D.MovementControl movementControl =
            CharacterMotor2D.MovementControl.Locked;

        public BloodSwordStep(string suffix)
        {
            animatorTrigger = "BloodSword" + suffix;
            animatorState = "Playerable_Mayhem_Skill_Animation_004_BloodSword_" + suffix;
        }
    }

    [Header("References")]
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform effectSpawnPoint;

    [Header("Blood Sword")]
    [SerializeField] private BloodSwordStep bloodSword00 = new("00");
    [SerializeField] private BloodSwordStep bloodSword01 = new("01");
    [SerializeField] private BloodSwordStep bloodSword02 = new("02");

    private const float ComboWindow = 1f;
    private const int EffectSpawnFrame = 2;
    private readonly List<GameObject> effects = new();
    private BloodSwordStep nextStep;
    private BloodSwordStep activeStep;
    private Coroutine attackCoroutine;
    private float lastAttackStartedAt = float.NegativeInfinity;
    private bool attackQueued;
    private float queuedInputAt;
    private bool motionFinished;
    private bool motionInterrupted;
    private bool effectSpawned;
    private bool ownsMovementControl;
    private CharacterMotor2D.MovementControl previousMovementControl;

    public bool IsActing => activeStep != null;

    public void RequestBasicAttack()
    {
        if (!isActiveAndEnabled || motor == null)
            return;

        if (IsActing)
        {
            if (!attackQueued)
            {
                attackQueued = true;
                queuedInputAt = Time.time;
            }
            return;
        }

        if (BeginAttack(Time.time))
            attackCoroutine = StartCoroutine(RunAttack());
    }

    private bool BeginAttack(float inputAt)
    {
        // Recheck at execution time, including buffered input after the previous motion.
        if (!motor.IsGrounded)
            return false;

        BloodSwordStep step = nextStep == null || inputAt - lastAttackStartedAt > ComboWindow
            ? bloodSword00
            : nextStep;

        if (!HasRequiredReferences(step))
        {
            Debug.LogWarning("Blood Sword requires its motion, effect, Animator and spawn references.", this);
            return false;
        }

        if (!ownsMovementControl)
        {
            previousMovementControl = motor.CurrentMovementControl;
            ownsMovementControl = true;
        }
        activeStep = step;
        motionFinished = false;
        motionInterrupted = false;
        effectSpawned = false;
        motor.SetMovementControl(step.movementControl);

        try
        {
            animator.SetTrigger(step.animatorTrigger);
        }
        catch (Exception exception)
        {
            CleanupAttack();
            Debug.LogException(exception, this);
            return false;
        }

        lastAttackStartedAt = Time.time;
        nextStep = step == bloodSword00 ? bloodSword01
            : step == bloodSword01 ? bloodSword02
            : bloodSword00;
        return true;
    }

    private bool HasRequiredReferences(BloodSwordStep step)
    {
        return step != null && animator != null && animator.isActiveAndEnabled &&
            animator.runtimeAnimatorController != null && effectSpawnPoint != null &&
            animator.GetBehaviours<BloodSwordMotionRelay>().Length > 0 &&
            step.animationClip != null && step.animationClip.length > 0f &&
            step.effectPrefab != null && step.effectClip != null && step.effectClip.length > 0f &&
            !string.IsNullOrEmpty(step.animatorTrigger) &&
            !string.IsNullOrEmpty(step.animatorState) &&
            animator.HasState(0, Animator.StringToHash("Base Layer." + step.animatorState));
    }

    private IEnumerator RunAttack()
    {
        while (IsActing)
        {
            // The presentation relay reports the actual end, including Animator speed.
            // Gameplay still owns buffering, completion, interruption and cleanup.
            while (!motionFinished && !motionInterrupted)
            {
                if (animator == null || !animator.isActiveAndEnabled)
                {
                    motionInterrupted = true;
                    break;
                }
                yield return null;
            }

            if (motionInterrupted)
            {
                CancelAction();
                yield break;
            }

            bool runQueuedAttack = attackQueued;
            float inputAt = queuedInputAt;
            attackQueued = false;
            CompleteAction(keepMovementControl: runQueuedAttack);

            if (!runQueuedAttack || !BeginAttack(inputAt))
            {
                // A queued attack can fail its ground/reference checks.
                CleanupAttack();
                break;
            }
        }

        attackCoroutine = null;
    }

    private void SpawnEffect(BloodSwordStep step)
    {
        effects.RemoveAll(effect => effect == null);
        float facing = motor.transform.localScale.x < 0f ? -1f : 1f;
        Vector3 spawnPosition = effectSpawnPoint.position + new Vector3(
            step.effectSpawnOffset.x * facing, step.effectSpawnOffset.y, 0f);
        GameObject effect = Instantiate(
            step.effectPrefab, spawnPosition, effectSpawnPoint.rotation);
        effects.Add(effect);

        Vector3 scale = effect.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * facing;
        effect.transform.localScale = scale;
        Destroy(effect, step.effectClip.length);
    }

    private void CompleteAction(bool keepMovementControl)
    {
        CleanupAttack(restoreMovementControl: !keepMovementControl);
    }

    public void NotifyBloodSwordMotionEntered(int stateHash)
    {
        if (MatchesActiveMotion(stateHash))
            lastAttackStartedAt = Time.time;
    }

    public void NotifyBloodSwordMotionExited(int stateHash, bool completed)
    {
        if (!MatchesActiveMotion(stateHash))
            return;

        motionFinished = completed;
        motionInterrupted = !completed;
    }

    public void NotifyBloodSwordMotionUpdated(int stateHash, float normalizedTime)
    {
        if (!isActiveAndEnabled || !MatchesActiveMotion(stateHash) || effectSpawned ||
            motionFinished || motionInterrupted)
            return;

        // Animation window 0:02 means frame 2, not a fixed wall-clock delay.
        AnimationClip clip = activeStep.animationClip;
        float effectTime = EffectSpawnFrame / clip.frameRate;
        if (normalizedTime * clip.length < effectTime)
            return;

        effectSpawned = true;
        try
        {
            SpawnEffect(activeStep);
        }
        catch (Exception exception)
        {
            CancelAction();
            Debug.LogException(exception, this);
        }
    }

    private bool MatchesActiveMotion(int stateHash)
    {
        return activeStep != null && stateHash ==
            Animator.StringToHash("Base Layer." + activeStep.animatorState);
    }

    public void CancelAction()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackQueued = false;
        CleanupAttack();
        foreach (GameObject effect in effects)
        {
            if (effect != null)
                Destroy(effect);
        }
        effects.Clear();
    }

    private void CleanupAttack(bool restoreMovementControl = true)
    {
        if (activeStep != null && animator != null)
            animator.ResetTrigger(activeStep.animatorTrigger);

        if (restoreMovementControl)
        {
            if (ownsMovementControl && motor != null)
                motor.SetMovementControl(previousMovementControl);

            ownsMovementControl = false;
        }
        activeStep = null;
        motionFinished = false;
        motionInterrupted = false;
        effectSpawned = false;
    }

    private void OnDisable()
    {
        CancelAction();
    }
}
