using UnityEngine;

// Presentation notifications only; MayhemMechanism owns the whole action and dash distance.
public class BloodSpearMotionRelay : StateMachineBehaviour
{
    private MayhemMechanism mechanism;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism = animator.GetComponentInParent<MayhemMechanism>();
        mechanism?.NotifyBloodSpearMotionEntered(stateInfo.fullPathHash);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodSpearMotionUpdated(stateInfo.fullPathHash, stateInfo.normalizedTime);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism?.NotifyBloodSpearMotionExited(stateInfo.fullPathHash, stateInfo.normalizedTime >= 1f);
    }
}
