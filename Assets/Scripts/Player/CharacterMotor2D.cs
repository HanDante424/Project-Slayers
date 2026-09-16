using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class CharacterMotor2D : MonoBehaviour
{
    public enum MovementControl
    {
        Free,
        Reduced,
        Locked
    }

    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.08f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    private bool isGrounded;

    [SerializeField] private MovementControl movementControl = MovementControl.Free;
    [SerializeField] private float reducedMoveMultiplier = 0.5f;

    // 나중에 스킬/넉백과 분리하기 위한 속도
    private float inputVelocityX;
    private Vector2 skillVelocity;
    private Vector2 knockbackVelocity;
    private bool hitStunLocked;

    public bool IsGrounded => isGrounded;
    public float MoveInputX => inputReader.MoveInput.x;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        HandleInput();
        HandleJump();
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyVelocity();
    }

    private void HandleInput()
    {
        float moveInput = inputReader.MoveInput.x;

        if (hitStunLocked)
        {
            inputVelocityX = 0f;
        }
        else switch (movementControl)
        {
            case MovementControl.Free:
                inputVelocityX = moveInput * moveSpeed;
                break;

            case MovementControl.Reduced:
                inputVelocityX =
                    moveInput *
                    moveSpeed *
                    reducedMoveMultiplier;
                break;

            case MovementControl.Locked:
                inputVelocityX = 0f;
                break;
        }

        if (!IsMovementLocked)
        {
            if (inputReader.MoveInput.x > 0.01f)
            {
                FaceRight();
            }
            else if (inputReader.MoveInput.x < -0.01f)
            {
                FaceLeft();
            }
        }
    }

    private void HandleJump()
    {
        if (!inputReader.ConsumeJump())
            return;

        if (IsMovementLocked)
            return;

        if (!isGrounded)
            return;

        isGrounded = false;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            0f
        );

        rb.AddForce(
            Vector2.up * jumpForce,
            ForceMode2D.Impulse
        );
    }

    private void ApplyVelocity()
    {
        float finalX =
            inputVelocityX +
            skillVelocity.x +
            knockbackVelocity.x;

        rb.linearVelocity = new Vector2(
            finalX,
            rb.linearVelocity.y
        );
    }

    private void CheckGround()
    {
        Bounds bounds = capsuleCollider.bounds;

        Vector2 checkSize = new Vector2(
            bounds.size.x * 0.8f,
            0.05f
        );

        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y + 0.02f
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            checkPosition,
            checkSize,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        isGrounded = hit.collider != null;
    }

    private void FaceRight()
    {
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void FaceLeft()
    {
        transform.localScale = new Vector3(
            -Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider2D capsule =
            GetComponent<CapsuleCollider2D>();

        if (capsule == null)
            return;

        Bounds bounds = capsule.bounds;

        Vector2 checkSize = new Vector2(
            bounds.size.x * 0.8f,
            0.05f
        );

        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y + 0.02f
        );

        Gizmos.DrawWireCube(
            checkPosition +
            Vector2.down * groundCheckDistance,
            checkSize
        );
    }

    public void SetMovementControl(MovementControl control)
    {
        movementControl = control;
    }

    public void SetHitStunLocked(bool locked)
    {
        hitStunLocked = locked;

        if (locked)
            inputVelocityX = 0f;
    }

    private bool IsMovementLocked =>
        hitStunLocked ||
        movementControl == MovementControl.Locked;
}
