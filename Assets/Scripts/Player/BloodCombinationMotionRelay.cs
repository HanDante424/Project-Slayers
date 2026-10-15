using UnityEngine;

// The shared automatic-sequence runtime owns progression and cleanup; this relays presentation timing.
public class BloodCombinationMotionRelay : StateMachineBehaviour
{
    private MayhemMechanism mechanism;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism = animator.GetComponentInParent<MayhemMechanism>();
        mechanism?.NotifyBloodCombinationMotionEntered(stateInfo.fullPathHash);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodCombinationMotionUpdated(stateInfo.fullPathHash, stateInfo.normalizedTime);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodCombinationMotionExited(stateInfo.fullPathHash, stateInfo.normalizedTime >= 1f);
    }
}
