using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private int hitIndex;
    [SerializeField] private GameObject source;

    private void OnTriggerEnter2D(Collider2D other)
    {
        HitReceiver receiver =
            other.GetComponentInParent<HitReceiver>();

        if (receiver == null)
            return;

        HitData hitData = new HitData
        {
            damage = damage,
            hitIndex = hitIndex,
            source = source
        };

        receiver.ReceiveHit(hitData);
    }
}