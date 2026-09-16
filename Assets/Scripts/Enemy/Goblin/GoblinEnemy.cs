using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class GoblinEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AttackHitboxController attackHitboxController;
    [SerializeField] private Transform attackHitboxTransform;

    [Header("Attack")]
    [SerializeField] private float attack = 2f;
    [SerializeField] private SkillData basicAttackSkillData;
    [SerializeField] private float attackWindup = 1.5f;
    [SerializeField] private float attackMovementLockDuration = 1.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 7f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Reaction")]
    [SerializeField] private float toughness = 0f;

    [Header("Knockback")]
    [SerializeField] private float knockbackSpeed = 4f;
    [SerializeField] private float knockbackDuration = 0.12f;

    [Header("Death")]
    [SerializeField] private float deathDespawnDelay = 1.5f;

    private Rigidbody2D rb;
    private Health health;
    private GoblinAnimationController animationController;
    private Transform target;

    private Action<GoblinEnemy> releaseAction;
    private Coroutine deathDespawnCoroutine;
    private Coroutine hitStunCoroutine;
    private Coroutine knockbackCoroutine;
    private Coroutine attackCoroutine;
    private RigidbodyConstraints2D constraintsBeforeDeath;

    private bool targetDetected;
    private bool isHitStunned;
    private bool isKnockedBack;
    private bool rigidbodyFrozenForDeath;
    private bool isAttacking;
    private float attackHitboxOffsetX;

    public float Toughness => toughness;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        animationController = GetComponent<GoblinAnimationController>();
        health.Died += HandleDeath;

        if (attackHitboxTransform != null)
            attackHitboxOffsetX = Mathf.Abs(attackHitboxTransform.localPosition.x);
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

        if (!isAttacking)
            UpdateDirection();

        TryStartAttack();
    }

    private void FixedUpdate()
    {
        if (health.IsDead)
            return;

        if (isKnockedBack || isHitStunned || isAttacking)
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

        CancelKnockback();
        CancelHitStun();
        CancelAttack();

        health.ResetHealth();
        animationController.ResetForReuse();
        targetDetected = false;

        if (rigidbodyFrozenForDeath)
        {
            rb.constraints = constraintsBeforeDeath;
            rigidbodyFrozenForDeath = false;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (attackHitboxController != null)
        {
            attackHitboxController.Initialize(
                attack,
                gameObject,
                basicAttackSkillData
            );
        }
    }

    public void ApplyHitStun(float duration)
    {
        if (health.IsDead || duration <= 0f)
            return;

        CancelHitStun();

        isHitStunned = true;
        attackHitboxController?.DisableAllHitboxes();

        if (!isKnockedBack)
            StopHorizontalMovement();

        hitStunCoroutine = StartCoroutine(
            EndHitStunAfterDuration(duration)
        );
    }

    private IEnumerator EndHitStunAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        hitStunCoroutine = null;
        isHitStunned = false;
    }

    private void CancelHitStun()
    {
        if (hitStunCoroutine != null)
        {
            StopCoroutine(hitStunCoroutine);
            hitStunCoroutine = null;
        }

        isHitStunned = false;
    }

    public void ApplyKnockback(float direction)
    {
        if (health.IsDead)
            return;

        float horizontalDirection = Mathf.Sign(direction);

        if (Mathf.Approximately(horizontalDirection, 0f))
            return;

        CancelKnockback();

        isKnockedBack = true;
        rb.linearVelocity = new Vector2(
            horizontalDirection * knockbackSpeed,
            rb.linearVelocity.y
        );

        knockbackCoroutine = StartCoroutine(
            EndKnockbackAfterDuration()
        );
    }

    private IEnumerator EndKnockbackAfterDuration()
    {
        yield return new WaitForSeconds(knockbackDuration);

        knockbackCoroutine = null;
        isKnockedBack = false;
        StopHorizontalMovement();
    }

    private void CancelKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        isKnockedBack = false;
    }

    private void HandleDeath()
    {
        CancelAttack();
        CancelKnockback();
        CancelHitStun();
        targetDetected = false;
        animationController.PlayDie();
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

    private void TryStartAttack()
    {
        if (isAttacking || isHitStunned || isKnockedBack ||
            !targetDetected || target == null)
        {
            return;
        }

        float horizontalDistance =
            target.position.x - transform.position.x;

        if (Mathf.Abs(horizontalDistance) > stopDistance)
            return;

        isAttacking = true;
        StopHorizontalMovement();
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(attackWindup);

        while (isHitStunned || isKnockedBack)
            yield return null;

        if (health.IsDead)
            yield break;

        animationController.PlayAttack();

        yield return new WaitForSeconds(
            attackMovementLockDuration
        );

        attackHitboxController?.DisableAllHitboxes();
        attackCoroutine = null;
        isAttacking = false;
    }

    private void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackHitboxController?.DisableAllHitboxes();
        isAttacking = false;
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
        SetAttackHitboxDirection(1f);
    }

    private void FaceLeft()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = true;
        SetAttackHitboxDirection(-1f);
    }

    private void SetAttackHitboxDirection(float direction)
    {
        if (attackHitboxTransform == null)
            return;

        Vector3 localPosition = attackHitboxTransform.localPosition;
        localPosition.x = attackHitboxOffsetX * direction;
        attackHitboxTransform.localPosition = localPosition;
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
