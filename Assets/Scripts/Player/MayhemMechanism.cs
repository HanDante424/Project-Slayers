using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MayhemMechanism : MonoBehaviour, ICharacterMechanism, ISecondaryCharacterMechanism, IBloodSpearMechanism, IBloodAxeMechanism, IBloodRampageMechanism
{
    [Serializable]
    private sealed class AttackStep
    {
        public string animatorTrigger;
        public string animatorState;
        public AnimationClip animationClip;
        public GameObject effectPrefab;
        public AnimationClip effectClip;
        [Min(0)] public int effectSpawnFrame = 2;
        [Tooltip("Combo retention from this attack's start. Frames use this motion clip's FPS; 12 frames at 12 FPS = 1 second. Not an attack cancel window.")]
        [Min(0)] public int comboGraceFrames = 12;
        [Tooltip("First motion clip frame accepting a queued input. Frame 0 is the motion start.")]
        [Min(0)] public int inputBufferStartFrame = 0;
        [Tooltip("Last motion clip frame accepting a queued input (inclusive). -1 means until motion completion.")]
        [Min(-1)] public int inputBufferEndFrame = -1;
        public Vector2 effectSpawnOffset = Vector2.zero;
        public CharacterMotor2D.MovementControl movementControl =
            CharacterMotor2D.MovementControl.Locked;

        public AttackStep(string suffix, bool bloodSlash = false)
        {
            animatorTrigger = (bloodSlash ? "BloodSlash" : "BloodSword") + suffix;
            animatorState = (bloodSlash ? "Playerable_Mayhem_Skill_Motion_010_BloodSlash_"
                : "Playerable_Mayhem_Skill_Animation_004_BloodSword_") + suffix;
        }
    }

    private sealed class AttackCombo
    {
        public readonly AttackStep first;
        public readonly AttackStep second;
        public readonly AttackStep third;
        public AttackStep next;
        public float graceDuration;
        public float lastStartedAt = float.NegativeInfinity;

        public AttackCombo(AttackStep first, AttackStep second, AttackStep third)
        {
            this.first = first;
            this.second = second;
            this.third = third;
        }
    }

    [Header("References")]
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform effectSpawnPoint;

    [Header("Blood Sword")]
    [SerializeField] private AttackStep bloodSword00 = new("00");
    [SerializeField] private AttackStep bloodSword01 = new("01");
    [SerializeField] private AttackStep bloodSword02 = new("02");

    [Header("Blood Slash")]
    [SerializeField] private AttackStep bloodSlash00 = new("00", true);
    [SerializeField] private AttackStep bloodSlash01 = new("01", true);
    [SerializeField] private AttackStep bloodSlash02 = new("02", true);

    private readonly List<GameObject> effects = new();
    private AttackCombo swordCombo;
    private AttackCombo slashCombo;
    private AttackCombo activeCombo;
    private AttackStep activeStep;
    private Coroutine attackCoroutine;
    private readonly ActionInputBuffer<AttackCombo> attackBuffer = new();
    private bool motionFinished;
    private bool motionInterrupted;
    private bool effectSpawned;
    private int currentMotionFrame;
    private bool ownsMovementControl;
    private CharacterMotor2D.MovementControl previousMovementControl;

    public bool IsActing => activeStep != null || IsSpearActing || IsAxeActing || IsRampageActing;

    private void Awake()
    {
        swordCombo = new AttackCombo(bloodSword00, bloodSword01, bloodSword02);
        slashCombo = new AttackCombo(bloodSlash00, bloodSlash01, bloodSlash02);
        InitializeBloodRampage();
    }

    public void RequestBasicAttack() => RequestAttack(swordCombo);
    public void RequestSecondaryAttack() => RequestAttack(slashCombo);

    private void Update()
    {
        UpdateBloodSpear();
        UpdateBloodAxe();
        UpdateBloodRampage();
    }
    private void FixedUpdate() => FixedUpdateBloodSpear();

    public void RequestBloodSpear()
    {
        if (!isActiveAndEnabled || IsActing)
            return;

        attackBuffer.Clear();
        TryStartBloodSpear();
    }

    private void RequestAttack(AttackCombo combo)
    {
        if (!isActiveAndEnabled || motor == null)
            return;
        if (IsSpearActing || IsAxeActing || IsRampageActing)
            return;

        if (IsActing)
        {
            // Both attack kinds share the current motion's acceptance window.
            if (!IsInputBufferOpen())
                return;
            attackBuffer.TryQueue(combo, Time.time);
            return;
        }

        if (BeginAttack(combo, Time.time))
            attackCoroutine = StartCoroutine(RunAttack());
    }

    private bool BeginAttack(AttackCombo combo, float inputAt)
    {
        // Recheck at execution time, including buffered input after the previous motion.
        if (!motor.IsGrounded)
            return false;

        AttackStep step = combo.next == null || inputAt - combo.lastStartedAt > combo.graceDuration
            ? combo.first
            : combo.next;

        if (!HasRequiredReferences(step))
        {
            Debug.LogWarning("Mayhem attack requires its motion, effect, Animator and spawn references.", this);
            return false;
        }

        if (!ownsMovementControl)
        {
            previousMovementControl = motor.CurrentMovementControl;
            ownsMovementControl = true;
        }
        activeStep = step;
        activeCombo = combo;
        motionFinished = false;
        motionInterrupted = false;
        effectSpawned = false;
        currentMotionFrame = 0;
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

        combo.lastStartedAt = Time.time;
        combo.graceDuration = Mathf.Max(0, step.comboGraceFrames) / step.animationClip.frameRate;
        combo.next = step == combo.first ? combo.second
            : step == combo.second ? combo.third
            : combo.first;
        return true;
    }

    private bool HasRequiredReferences(AttackStep step)
    {
        return step != null && animator != null && animator.isActiveAndEnabled &&
            animator.runtimeAnimatorController != null && effectSpawnPoint != null &&
            animator.GetBehaviours<BloodSwordMotionRelay>().Length > 0 &&
            step.animationClip != null && step.animationClip.length > 0f && step.animationClip.frameRate > 0f &&
            step.effectPrefab != null && step.effectClip != null && step.effectClip.length > 0f &&
            !string.IsNullOrEmpty(step.animatorTrigger) &&
            !string.IsNullOrEmpty(step.animatorState) &&
            animator.HasState(0, Animator.StringToHash("Base Layer." + step.animatorState));
    }

    private bool IsInputBufferOpen()
    {
        return !motionFinished && !motionInterrupted &&
            currentMotionFrame >= activeStep.inputBufferStartFrame &&
            (activeStep.inputBufferEndFrame < 0 || currentMotionFrame <= activeStep.inputBufferEndFrame);
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

            bool runQueuedAttack = attackBuffer.TryConsume(out AttackCombo combo, out float inputAt);
            CompleteAction(keepMovementControl: runQueuedAttack);

            if (!runQueuedAttack || !BeginAttack(combo, inputAt))
            {
                // A queued attack can fail its ground/reference checks.
                CleanupAttack();
                break;
            }
        }

        attackCoroutine = null;
    }

    private void SpawnEffect(AttackStep step)
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
            activeCombo.lastStartedAt = Time.time;
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
        if (!isActiveAndEnabled || !MatchesActiveMotion(stateHash) ||
            motionFinished || motionInterrupted)
            return;

        // Animation window 0:02 means frame 2, not a fixed wall-clock delay.
        AnimationClip clip = activeStep.animationClip;
        // Track the displayed clip frame even after the effect has been emitted.
        currentMotionFrame = Mathf.FloorToInt(normalizedTime * clip.length * clip.frameRate + 0.0001f);
        if (effectSpawned)
            return;
        float effectTime = activeStep.effectSpawnFrame / clip.frameRate;
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
        CancelBloodRampage();
        CancelBloodAxe();
        CancelBloodSpear();
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackBuffer.Clear();
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
        activeCombo = null;
        currentMotionFrame = 0;
        motionFinished = false;
        motionInterrupted = false;
        effectSpawned = false;
    }

    private void OnDisable()
    {
        CancelAction();
    }
}
