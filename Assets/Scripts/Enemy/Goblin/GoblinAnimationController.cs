using UnityEngine;

public class GoblinAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Health health;

    [Header("Settings")]
    [SerializeField] private float moveThreshold = 0.01f;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

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
        animator.SetTrigger(
            HitHash
        );
    }
}