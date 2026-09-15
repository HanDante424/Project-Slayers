using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GoblinEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private GoblinAnimationController animationController;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 7f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.8f;

    private Rigidbody2D rb;
    private Transform target;

    private Action<GoblinEnemy> releaseAction;

    private bool targetDetected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckTarget();
        UpdateDirection();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void Initialize(
        Transform newTarget,
        Action<GoblinEnemy> newReleaseAction)
    {
        target = newTarget;
        releaseAction = newReleaseAction;

        targetDetected = false;

        rb.linearVelocity = Vector2.zero;
    }

    private void CheckTarget()
    {
        if (target == null)
        {
            targetDetected = false;
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            target.position
        );

        targetDetected = distance <= detectionRange;
    }

    private void Move()
    {
        if (!targetDetected || target == null)
        {
            StopHorizontalMovement();
            return;
        }

        float horizontalDistance =
            target.position.x - transform.position.x;

        if (Mathf.Abs(horizontalDistance) <= stopDistance)
        {
            StopHorizontalMovement();
            return;
        }

        float direction = Mathf.Sign(horizontalDistance);

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void StopHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private void UpdateDirection()
    {
        if (!targetDetected || target == null)
            return;

        float horizontalDistance =
            target.position.x - transform.position.x;

        if (horizontalDistance > 0.01f)
        {
            FaceRight();
        }
        else if (horizontalDistance < -0.01f)
        {
            FaceLeft();
        }
    }

    private void FaceRight()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = false;
    }

    private void FaceLeft()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = true;
    }

    public void Despawn()
    {
        releaseAction?.Invoke(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );
    }

    public void ReceiveTestHit()
    {
        Debug.Log($"{name} hit");

        if (animationController != null)
            animationController.PlayHit();
    }
}