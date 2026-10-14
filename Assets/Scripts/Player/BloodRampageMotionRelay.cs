using UnityEngine;

// Presentation notifications; the mechanism owns the uninterrupted seven-step action.
public class BloodRampageMotionRelay : StateMachineBehaviour
{
    private MayhemMechanism mechanism;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism = animator.GetComponentInParent<MayhemMechanism>();
        mechanism?.NotifyBloodRampageMotionEntered(stateInfo.fullPathHash);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodRampageMotionUpdated(stateInfo.fullPathHash, stateInfo.normalizedTime);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodRampageMotionExited(stateInfo.fullPathHash, stateInfo.normalizedTime >= 1f);
    }
}
