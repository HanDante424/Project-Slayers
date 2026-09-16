using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        HitReceiver receiver =
            other.GetComponentInParent<HitReceiver>();

        if (receiver == null)
            return;

        receiver.ReceiveHit();
    }
}