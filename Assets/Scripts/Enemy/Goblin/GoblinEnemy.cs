using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class GoblinEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 7f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Death")]
    [SerializeField] private float deathDespawnDelay = 1.5f;

    private Rigidbody2D rb;
    private Health health;
    private Transform target;

    private Action<GoblinEnemy> releaseAction;
    private Coroutine deathDespawnCoroutine;
    private RigidbodyConstraints2D constraintsBeforeDeath;

    private bool targetDetected;
    private bool rigidbodyFrozenForDeath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        health.Died += HandleDeath;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.Died -= HandleDeath;
    }

    private void Update()
    {
        if (health.IsDead)
            return;

        CheckTarget();
        UpdateDirection();
    }

    private void FixedUpdate()
    {
        if (health.IsDead)
            return;

        Move();
    }

    public void Initialize(
        Transform newTarget,
        Action<GoblinEnemy> newReleaseAction)
    {
        target = newTarget;
        releaseAction = newReleaseAction;

        if (deathDespawnCoroutine != null)
        {
            StopCoroutine(deathDespawnCoroutine);
            deathDespawnCoroutine = null;
        }

        health.ResetHealth();
        targetDetected = false;

        if (rigidbodyFrozenForDeath)
        {
            rb.constraints = constraintsBeforeDeath;
            rigidbodyFrozenForDeath = false;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void HandleDeath()
    {
        targetDetected = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        constraintsBeforeDeath = rb.constraints;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rigidbodyFrozenForDeath = true;

        if (deathDespawnCoroutine == null)
        {
            deathDespawnCoroutine = StartCoroutine(
                DespawnAfterDeathDelay()
            );
        }
    }

    private IEnumerator DespawnAfterDeathDelay()
    {
        yield return new WaitForSeconds(deathDespawnDelay);

        deathDespawnCoroutine = null;
        Despawn();
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

}
