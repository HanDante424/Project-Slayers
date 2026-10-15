using UnityEngine;

public partial class MayhemMechanism
{
    // Keep Rampage's existing inline serialized step layout; both automatic sequences use it.
    [Header("Blood Combination")]
    [SerializeField] private RampageStep bloodCombination00 = new("00", 2, true);
    [SerializeField] private RampageStep bloodCombination01 = new("01", 2, true);
    [SerializeField] private RampageStep bloodCombination02 = new("02", 2, true);

    private static readonly int CombinationPlayingHash = Animator.StringToHash("BloodCombinationPlaying");
    private RampageStep[] combinationSteps;
    private bool IsCombinationActing => rampagePhase != RampagePhase.Inactive &&
        activeSequenceKind == AutomaticSequenceKind.BloodCombination;

    private void InitializeBloodCombination()
    {
        combinationSteps = new[] { bloodCombination00, bloodCombination01, bloodCombination02 };
    }

    public void RequestBloodCombination() => TryStartAutomaticSequence(combinationSteps,
        AutomaticSequenceKind.BloodCombination, CombinationPlayingHash, typeof(BloodCombinationMotionRelay));

    public void NotifyBloodCombinationMotionEntered(int stateHash)
    {
        if (IsCombinationActing)
            NotifySequenceMotionEntered(stateHash);
    }

    public void NotifyBloodCombinationMotionUpdated(int stateHash, float normalizedTime)
    {
        if (IsCombinationActing)
            NotifySequenceMotionUpdated(stateHash, normalizedTime);
    }

    public void NotifyBloodCombinationMotionExited(int stateHash, bool completed)
    {
        if (IsCombinationActing)
            NotifySequenceMotionExited(stateHash, completed);
    }

    private void CancelBloodCombination()
    {
        if (IsCombinationActing)
            CleanupAutomaticSequence();
    }
}
