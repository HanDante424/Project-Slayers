using System.Collections;
using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private CharacterMotor2D motor;

    private Coroutine hitStunCoroutine;

    public void ReceiveHit(HitData hitData)
    {
        if (health.IsDead)
            return;

        Debug.Log(
            $"Player hitIndex: {hitData.hitIndex}, damage: {hitData.damage}",
            this
        );

        health.TakeDamage(hitData.damage);

        if (health.IsDead)
            return;

        animationController.PlayHit();
        ApplyHitStun(hitData.hitStunDuration);
    }

    private void ApplyHitStun(float duration)
    {
        if (duration <= 0f)
            return;

        CancelHitStun();
        motor.SetHitStunLocked(true);
        hitStunCoroutine = StartCoroutine(
            EndHitStunAfterDuration(duration)
        );
    }

    private IEnumerator EndHitStunAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        hitStunCoroutine = null;
        motor.SetHitStunLocked(false);
    }

    private void OnDisable()
    {
        CancelHitStun();
    }

    private void CancelHitStun()
    {
        if (hitStunCoroutine != null)
        {
            StopCoroutine(hitStunCoroutine);
            hitStunCoroutine = null;
        }

        if (motor != null)
            motor.SetHitStunLocked(false);
    }
}
