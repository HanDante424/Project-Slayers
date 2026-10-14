using UnityEngine;

// Presentation only: the mechanism owns action exclusivity, stage progression and cleanup.
public class BloodAxeMotionRelay : StateMachineBehaviour
{
    private MayhemMechanism mechanism;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism = animator.GetComponentInParent<MayhemMechanism>();
        mechanism?.NotifyBloodAxeMotionEntered(stateInfo.fullPathHash);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodAxeMotionUpdated(stateInfo.fullPathHash, stateInfo.normalizedTime);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodAxeMotionExited(stateInfo.fullPathHash, stateInfo.normalizedTime >= 1f);
    }
}
