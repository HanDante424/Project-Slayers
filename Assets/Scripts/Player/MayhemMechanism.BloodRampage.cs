using System;
using UnityEngine;

public partial class MayhemMechanism
{
    [Serializable]
    private sealed class RampageStep
    {
        public AnimationClip motionAnimationClip;
        public string animatorState;
        public string animatorTrigger;
        public GameObject effectPrefab;
        public AnimationClip effectAnimationClip;
        public Vector2 effectSpawnOffset;
        [Min(0)] public int effectSpawnFrame;

        public RampageStep(string suffix, int spawnFrame = 0, bool combination = false)
        {
            animatorState = (combination ? "Playerable_Mayhem_Skill_Motion_023_BloodCombination_"
                : "Playerable_Mayhem_Skill_Motion_020_BloodRampage_") + suffix;
            animatorTrigger = (combination ? "BloodCombination" : "BloodRampage") + suffix;
            effectSpawnFrame = spawnFrame;
        }
    }

    [Header("Blood Rampage")]
    [SerializeField] private RampageStep bloodRampage00 = new("00", 2);
    [SerializeField] private RampageStep bloodRampage01 = new("01");
    [SerializeField] private RampageStep bloodRampage02 = new("02");
    [SerializeField] private RampageStep bloodRampage03 = new("03");
    [SerializeField] private RampageStep bloodRampage04 = new("04");
    [SerializeField] private RampageStep bloodRampage05 = new("05");
    [SerializeField] private RampageStep bloodRampage06 = new("06");

    private enum RampagePhase { Inactive, WaitingEntry, Playing, WaitingNext }
    private enum AutomaticSequenceKind { None, BloodRampage, BloodCombination }
    private static readonly int RampagePlayingHash = Animator.StringToHash("BloodRampagePlaying");
    private RampageStep[] rampageSteps;
    // Shared runtime only. Existing serialized Rampage fields and their layout remain unchanged.
    private RampageStep[] activeSequenceSteps;
    private AutomaticSequenceKind activeSequenceKind;
    private int activeSequencePlayingHash;
    private RampagePhase rampagePhase;
    // This is the documented 00..06 sequence position, not a numeric skill identity.
    private int rampageStepIndex;
    private bool rampageEffectSpawned;
    private float rampageFacing;
    private Health rampageHealth;
    private CharacterMotor2D.MovementControl rampagePreviousMovement;
    private bool IsRampageActing => rampagePhase != RampagePhase.Inactive &&
        activeSequenceKind == AutomaticSequenceKind.BloodRampage;

    private void InitializeBloodRampage()
    {
        rampageSteps = new[] { bloodRampage00, bloodRampage01, bloodRampage02, bloodRampage03,
            bloodRampage04, bloodRampage05, bloodRampage06 };
        rampageHealth = GetComponent<Health>();
    }

    public void RequestBloodRampage() => TryStartAutomaticSequence(rampageSteps,
        AutomaticSequenceKind.BloodRampage, RampagePlayingHash, typeof(BloodRampageMotionRelay));

    private void TryStartAutomaticSequence(RampageStep[] steps, AutomaticSequenceKind kind, int playingHash, Type relayType)
    {
        if (!isActiveAndEnabled || IsActing || motor == null || !motor.isActiveAndEnabled ||
            !motor.IsGrounded || motor.IsHitStunLocked || (rampageHealth != null && rampageHealth.IsDead))
            return;
        if (!HasSequenceReferences(steps, relayType))
        {
            Debug.LogWarning($"{kind} requires its motion/effect references, reachable effect frames, Animator and spawn point.", this);
            return;
        }

        attackBuffer.Clear();
        activeSequenceSteps = steps;
        activeSequenceKind = kind;
        activeSequencePlayingHash = playingHash;
        rampagePreviousMovement = motor.CurrentMovementControl;
        rampageFacing = motor.transform.localScale.x < 0f ? -1f : 1f;
        rampageStepIndex = 0;
        rampageEffectSpawned = false;
        rampagePhase = RampagePhase.WaitingEntry;
        if (rampageHealth != null)
            rampageHealth.Died += CancelAutomaticSequence;
        motor.SetMovementControl(CharacterMotor2D.MovementControl.Locked);
        animator.SetBool(activeSequencePlayingHash, true);
        animator.SetTrigger(steps[0].animatorTrigger);
    }

    private bool HasSequenceReferences(RampageStep[] steps, Type relayType)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null ||
            effectSpawnPoint == null || steps == null || steps.Length == 0)
            return false;
        int relayCount = 0;
        foreach (StateMachineBehaviour relay in animator.GetBehaviours<StateMachineBehaviour>())
            if (relay.GetType() == relayType) relayCount++;
        if (relayCount != steps.Length)
            return false;
        foreach (RampageStep step in steps)
        {
            if (step == null || step.motionAnimationClip == null || step.motionAnimationClip.length <= 0f ||
                step.motionAnimationClip.frameRate <= 0f || step.effectSpawnFrame < 0 ||
                step.effectSpawnFrame > step.motionAnimationClip.length * step.motionAnimationClip.frameRate + .0001f ||
                string.IsNullOrEmpty(step.animatorState) ||
                string.IsNullOrEmpty(step.animatorTrigger) || !animator.HasState(0, RampageStateHash(step)) ||
                step.effectPrefab == null || step.effectAnimationClip == null || step.effectAnimationClip.length <= 0f)
                return false;
            Animator effectAnimator = step.effectPrefab.GetComponent<Animator>();
            if (effectAnimator == null || effectAnimator.runtimeAnimatorController == null ||
                !Array.Exists(effectAnimator.runtimeAnimatorController.animationClips, clip => clip == step.effectAnimationClip))
                return false;
        }
        return true;
    }

    private static int RampageStateHash(RampageStep step) => Animator.StringToHash("Base Layer." + step.animatorState);

    public void NotifyBloodRampageMotionEntered(int stateHash)
    {
        if (IsRampageActing)
            NotifySequenceMotionEntered(stateHash);
    }

    private void NotifySequenceMotionEntered(int stateHash)
    {
        if (!isActiveAndEnabled || rampagePhase == RampagePhase.Inactive)
            return;
        int expected = rampagePhase == RampagePhase.WaitingNext ? rampageStepIndex + 1 : rampageStepIndex;
        if ((rampagePhase != RampagePhase.WaitingEntry && rampagePhase != RampagePhase.WaitingNext) ||
            expected >= activeSequenceSteps.Length || stateHash != RampageStateHash(activeSequenceSteps[expected]))
        {
            CancelAutomaticSequence();
            return;
        }
        rampageStepIndex = expected;
        rampagePhase = RampagePhase.Playing;
        rampageEffectSpawned = false;
        if (activeSequenceSteps[expected].effectSpawnFrame == 0)
            EmitRampageEffect();
    }

    public void NotifyBloodRampageMotionUpdated(int stateHash, float normalizedTime)
    {
        if (IsRampageActing)
            NotifySequenceMotionUpdated(stateHash, normalizedTime);
    }

    private void NotifySequenceMotionUpdated(int stateHash, float normalizedTime)
    {
        if (rampagePhase != RampagePhase.Playing || stateHash != RampageStateHash(activeSequenceSteps[rampageStepIndex]))
            return;
        RampageStep step = activeSequenceSteps[rampageStepIndex];
        // Same frame-2 convention as Blood Sword; process the last frame before requesting the next motion.
        if (!rampageEffectSpawned && normalizedTime * step.motionAnimationClip.length >=
            step.effectSpawnFrame / step.motionAnimationClip.frameRate)
            EmitRampageEffect();
        if (rampagePhase == RampagePhase.Inactive)
            return;
        if (normalizedTime >= 1f && rampageStepIndex + 1 < activeSequenceSteps.Length)
        {
            rampagePhase = RampagePhase.WaitingNext;
            animator.SetTrigger(activeSequenceSteps[rampageStepIndex + 1].animatorTrigger);
        }
    }

    public void NotifyBloodRampageMotionExited(int stateHash, bool completed)
    {
        if (IsRampageActing)
            NotifySequenceMotionExited(stateHash, completed);
    }

    private void NotifySequenceMotionExited(int stateHash, bool completed)
    {
        if (rampagePhase == RampagePhase.Inactive || stateHash != RampageStateHash(activeSequenceSteps[rampageStepIndex]))
            return;
        // A non-looping state's final update can be replaced by its exit callback.
        // All configured frames are validated as reachable; normal exit confirms the end frame was sampled.
        if (completed && !rampageEffectSpawned)
            EmitRampageEffect();
        if (rampagePhase == RampagePhase.Inactive)
            return;
        if (completed && rampagePhase == RampagePhase.WaitingNext)
            return;
        if (completed && rampagePhase == RampagePhase.Playing && rampageStepIndex == activeSequenceSteps.Length - 1)
            CompleteAutomaticSequence();
        else
            CancelAutomaticSequence();
    }

    private void EmitRampageEffect()
    {
        if (rampageEffectSpawned)
            return;
        rampageEffectSpawned = true;
        GameObject instance = null;
        try
        {
            RampageStep step = activeSequenceSteps[rampageStepIndex];
            Vector3 offset = new(step.effectSpawnOffset.x * rampageFacing, step.effectSpawnOffset.y, 0f);
            instance = Instantiate(step.effectPrefab, effectSpawnPoint.position + offset, effectSpawnPoint.rotation);
            Vector3 scale = instance.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * rampageFacing;
            instance.transform.localScale = scale;
            // Like Axe Attack/Spear 01: spawned presentation finishes independently of action cleanup.
            instance.AddComponent<DestroyAfterAnimatorPlayback>();
        }
        catch (Exception exception)
        {
            if (instance != null) Destroy(instance);
            CancelAutomaticSequence();
            Debug.LogException(exception, this);
        }
    }

    private void UpdateBloodRampage()
    {
        if (rampagePhase != RampagePhase.Inactive && (animator == null || !animator.isActiveAndEnabled || motor == null ||
            !motor.isActiveAndEnabled || motor.IsHitStunLocked || (rampageHealth != null && rampageHealth.IsDead)))
            CancelAutomaticSequence();
    }

    private void CompleteAutomaticSequence() => CleanupAutomaticSequence();
    private void CancelAutomaticSequence() => CleanupAutomaticSequence();

    private void CancelBloodRampage()
    {
        if (IsRampageActing)
            CancelAutomaticSequence();
    }

    private void CleanupAutomaticSequence()
    {
        if (rampagePhase == RampagePhase.Inactive)
            return;
        if (rampageHealth != null)
            rampageHealth.Died -= CancelAutomaticSequence;
        if (animator != null)
        {
            foreach (RampageStep step in activeSequenceSteps)
                animator.ResetTrigger(step.animatorTrigger);
            animator.SetBool(activeSequencePlayingHash, false);
        }
        if (motor != null)
            motor.SetMovementControl(rampagePreviousMovement);
        attackBuffer.Clear();
        rampageEffectSpawned = false;
        rampagePhase = RampagePhase.Inactive;
        activeSequenceKind = AutomaticSequenceKind.None;
        activeSequenceSteps = null;
    }
}
