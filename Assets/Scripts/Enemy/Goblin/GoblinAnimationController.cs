using UnityEngine;

public class GoblinAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float moveThreshold = 0.01f;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

    private static readonly int DieHash =
        Animator.StringToHash("Die");

    private bool isDead;

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        bool isMoving =
            Mathf.Abs(rb.linearVelocity.x) > moveThreshold;

        animator.SetBool(
            IsMovingHash,
            isMoving
        );
    }

    public void PlayAttack()
    {
        animator.SetTrigger(
            AttackHash
        );
    }

    public void PlayHit()
    {
        if (isDead)
            return;

        animator.SetTrigger(
            HitHash
        );
    }

    public void PlayDie()
    {
        isDead = true;

        animator.ResetTrigger(HitHash);
        animator.SetTrigger(DieHash);
    }

    public void ResetForReuse()
    {
        isDead = false;
        animator.ResetTrigger(HitHash);
        animator.ResetTrigger(DieHash);
    }
}
