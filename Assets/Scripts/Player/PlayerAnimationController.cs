using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private Rigidbody2D rb;

    [Header("Animation Settings")]
    [SerializeField] private float moveThreshold = 0.01f;
    [SerializeField] private float verticalThreshold = 0.05f;

    // 방향 전환 시 순간적인 0 입력 때문에
    // Stand로 바뀌는 것을 방지하는 시간
    [SerializeField] private float moveGraceTime = 0.08f;

    private float moveGraceTimer;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int VerticalSpeedHash =
        Animator.StringToHash("VerticalSpeed");

    private void Update()
    {
        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        float moveInput = Mathf.Abs(motor.MoveInputX);
        float verticalSpeed = rb.linearVelocity.y;

        float animationSpeed;

        // 이동 입력이 존재하면
        // 달리기 판정을 일정 시간 유지한다.
        if (moveInput > moveThreshold)
        {
            moveGraceTimer = moveGraceTime;
            animationSpeed = moveInput;
        }
        else
        {
            moveGraceTimer -= Time.deltaTime;

            if (moveGraceTimer > 0f)
            {
                animationSpeed = 1f;
            }
            else
            {
                animationSpeed = 0f;
            }
        }

        if (Mathf.Abs(verticalSpeed) < verticalThreshold)
        {
            verticalSpeed = 0f;
        }

        animator.SetFloat(
            SpeedHash,
            animationSpeed
        );

        animator.SetBool(
            IsGroundedHash,
            motor.IsGrounded
        );

        animator.SetFloat(
            VerticalSpeedHash,
            verticalSpeed
        );
    }
}