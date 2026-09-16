using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int damagePercent = 100;
    [SerializeField] private int hitIndex;

    private float attack;
    private GameObject source;

    public void Initialize(
        float newAttack,
        GameObject newSource)
    {
        attack = newAttack;
        source = newSource;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HitReceiver receiver =
            other.GetComponentInParent<HitReceiver>();

        if (receiver == null)
            return;

        float baseDamage =
            attack * damagePercent / 100f;

        // TEMP: rounding rule unresolved.
        int damage = Mathf.RoundToInt(baseDamage);

        HitData hitData = new HitData
        {
            damage = damage,
            hitIndex = hitIndex,
            source = source
        };

        receiver.ReceiveHit(hitData);
    }
}
