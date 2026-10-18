using UnityEngine;

public class HitReceiver : MonoBehaviour
{
    [SerializeField] private GoblinAnimationController animationController;
    [SerializeField] private Health health;

    private GoblinEnemy goblinEnemy;

    public bool IsDead => health != null && health.IsDead;

    private void Awake()
    {
        goblinEnemy = GetComponent<GoblinEnemy>();
    }

    public void ReceiveHit(HitData hitData)
    {
        if (health != null && (health.IsDead || !health.DamageEnabled))
            return;

        Debug.Log(
            $"{name} hit / damage: {hitData.damage} / hitIndex: {hitData.hitIndex}"
        );

        if (health != null)
            health.TakeDamage(hitData.damage, hitData.source);

        if (health != null && health.IsDead)
            return;

        if (animationController != null)
            animationController.PlayHit();

        if (goblinEnemy != null && hitData.hitStunDuration > 0f)
        {
            goblinEnemy.ApplyHitStun(
                hitData.hitStunDuration
            );
        }

        if (goblinEnemy == null || hitData.source == null)
            return;

        if (hitData.knockbackPower <= goblinEnemy.Toughness)
            return;

        float horizontalOffset =
            goblinEnemy.transform.position.x -
            hitData.source.transform.position.x;

        if (Mathf.Approximately(horizontalOffset, 0f))
            return;

        float knockbackDirection =
            horizontalOffset > 0f ? 1f : -1f;

        goblinEnemy.ApplyKnockback(knockbackDirection);
    }
}
