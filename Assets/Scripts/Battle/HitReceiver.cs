using UnityEngine;

public class HitReceiver : MonoBehaviour
{
    [SerializeField] private GoblinAnimationController animationController;
    [SerializeField] private Health health;

    public void ReceiveHit(HitData hitData)
    {
        Debug.Log(
            $"{name} hit / damage: {hitData.damage} / hitIndex: {hitData.hitIndex}"
        );

        if (health != null)
            health.TakeDamage(hitData.damage);

        if (animationController != null)
            animationController.PlayHit();
    }
}