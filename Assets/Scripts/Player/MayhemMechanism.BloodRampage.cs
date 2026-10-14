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

        public RampageStep(string suffix, int spawnFrame = 0)
        {
            animatorState = "Playerable_Mayhem_Skill_Motion_020_BloodRampage_" + suffix;
            animatorTrigger = "BloodRampage" + suffix;
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
    private static readonly int RampagePlayingHash = Animator.StringToHash("BloodRampagePlaying");
    private RampageStep[] rampageSteps;
    private RampagePhase rampagePhase;
    // This is the documented 00..06 sequence position, not a numeric skill identity.
    private int rampageStepIndex;
    private bool rampageEffectSpawned;
    private float rampageFacing;
    private Health rampageHealth;
    private CharacterMotor2D.MovementControl rampagePreviousMovement;
    private bool IsRampageActing => rampagePhase != RampagePhase.Inactive;

    private void InitializeBloodRampage()
    {
        rampageSteps = new[] { bloodRampage00, bloodRampage01, bloodRampage02, bloodRampage03,
            bloodRampage04, bloodRampage05, bloodRampage06 };
        rampageHealth = GetComponent<Health>();
    }

    public void RequestBloodRampage()
    {
        if (!isActiveAndEnabled || IsActing || motor == null || !motor.isActiveAndEnabled ||
            !motor.IsGrounded || motor.IsHitStunLocked || (rampageHealth != null && rampageHealth.IsDead))
            return;
        if (!HasRampageReferences())
        {
            Debug.LogWarning("Blood Rampage requires all seven motion/effect references, Animator and spawn point.", this);
            return;
        }

        attackBuffer.Clear();
        rampagePreviousMovement = motor.CurrentMovementControl;
        rampageFacing = motor.transform.localScale.x < 0f ? -1f : 1f;
        rampageStepIndex = 0;
        rampageEffectSpawned = false;
        rampagePhase = RampagePhase.WaitingEntry;
        if (rampageHealth != null)
            rampageHealth.Died += CancelBloodRampage;
        motor.SetMovementControl(CharacterMotor2D.MovementControl.Locked);
        animator.SetBool(RampagePlayingHash, true);
        animator.SetTrigger(bloodRampage00.animatorTrigger);
    }

    private bool HasRampageReferences()
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null ||
            effectSpawnPoint == null || animator.GetBehaviours<BloodRampageMotionRelay>().Length != rampageSteps.Length)
            return false;
        foreach (RampageStep step in rampageSteps)
        {
            if (step == null || step.motionAnimationClip == null || step.motionAnimationClip.length <= 0f ||
                step.motionAnimationClip.frameRate <= 0f || string.IsNullOrEmpty(step.animatorState) ||
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
        if (!isActiveAndEnabled || !IsRampageActing)
            return;
        int expected = rampagePhase == RampagePhase.WaitingNext ? rampageStepIndex + 1 : rampageStepIndex;
        if ((rampagePhase != RampagePhase.WaitingEntry && rampagePhase != RampagePhase.WaitingNext) ||
            expected >= rampageSteps.Length || stateHash != RampageStateHash(rampageSteps[expected]))
        {
            CancelBloodRampage();
            return;
        }
        rampageStepIndex = expected;
        rampagePhase = RampagePhase.Playing;
        rampageEffectSpawned = false;
        if (rampageSteps[expected].effectSpawnFrame == 0)
            EmitRampageEffect();
    }

    public void NotifyBloodRampageMotionUpdated(int stateHash, float normalizedTime)
    {
        if (rampagePhase != RampagePhase.Playing || stateHash != RampageStateHash(rampageSteps[rampageStepIndex]))
            return;
        RampageStep step = rampageSteps[rampageStepIndex];
        // Same frame-2 convention as Blood Sword; process the last frame before requesting the next motion.
        if (!rampageEffectSpawned && normalizedTime * step.motionAnimationClip.length >=
            step.effectSpawnFrame / step.motionAnimationClip.frameRate)
            EmitRampageEffect();
        if (!IsRampageActing)
            return;
        if (normalizedTime >= 1f && rampageStepIndex + 1 < rampageSteps.Length)
        {
            rampagePhase = RampagePhase.WaitingNext;
            animator.SetTrigger(rampageSteps[rampageStepIndex + 1].animatorTrigger);
        }
    }

    public void NotifyBloodRampageMotionExited(int stateHash, bool completed)
    {
        if (!IsRampageActing || stateHash != RampageStateHash(rampageSteps[rampageStepIndex]))
            return;
        if (completed && rampagePhase == RampagePhase.WaitingNext)
            return;
        if (completed && rampagePhase == RampagePhase.Playing && rampageStepIndex == rampageSteps.Length - 1)
            CompleteBloodRampage();
        else
            CancelBloodRampage();
    }

    private void EmitRampageEffect()
    {
        if (rampageEffectSpawned)
            return;
        rampageEffectSpawned = true;
        GameObject instance = null;
        try
        {
            RampageStep step = rampageSteps[rampageStepIndex];
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
            CancelBloodRampage();
            Debug.LogException(exception, this);
        }
    }

    private void UpdateBloodRampage()
    {
        if (IsRampageActing && (animator == null || !animator.isActiveAndEnabled || motor == null ||
            !motor.isActiveAndEnabled || motor.IsHitStunLocked || (rampageHealth != null && rampageHealth.IsDead)))
            CancelBloodRampage();
    }

    private void CompleteBloodRampage() => CleanupBloodRampage();
    private void CancelBloodRampage() => CleanupBloodRampage();

    private void CleanupBloodRampage()
    {
        if (!IsRampageActing)
            return;
        if (rampageHealth != null)
            rampageHealth.Died -= CancelBloodRampage;
        if (animator != null)
        {
            foreach (RampageStep step in rampageSteps)
                animator.ResetTrigger(step.animatorTrigger);
            animator.SetBool(RampagePlayingHash, false);
        }
        if (motor != null)
            motor.SetMovementControl(rampagePreviousMovement);
        attackBuffer.Clear();
        rampageEffectSpawned = false;
        rampagePhase = RampagePhase.Inactive;
    }
}
