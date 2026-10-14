using UnityEngine;

// Presentation-only notifications; MayhemMechanism owns action and input state.
public class BloodSwordMotionRelay : StateMachineBehaviour
{
    [SerializeField] private bool interruptBloodSword;
    private static readonly int MotionPlayingHash = Animator.StringToHash("BloodSwordMotionPlaying");
    private MayhemMechanism mechanism;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mechanism = animator.GetComponentInParent<MayhemMechanism>();
        if (interruptBloodSword)
        {
            // Also cancel a requested attack whose trigger has not entered yet.
            mechanism?.CancelAction();
            return;
        }
        animator.SetBool(MotionPlayingHash, true);
        mechanism?.NotifyBloodSwordMotionEntered(stateInfo.fullPathHash);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!interruptBloodSword)
            mechanism?.NotifyBloodSwordMotionUpdated(stateInfo.fullPathHash, stateInfo.normalizedTime);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (interruptBloodSword)
            return;
        animator.SetBool(MotionPlayingHash, false);
        // Exit Time 1 is normal completion; Hit0 can exit earlier and must stay allowed.
        mechanism?.NotifyBloodSwordMotionExited(stateInfo.fullPathHash, stateInfo.normalizedTime >= 1f);
    }
}
