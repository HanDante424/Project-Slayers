using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int hitIndex;

    private float attack;
    private GameObject source;
    private SkillData skillData;

    public void Initialize(
        float newAttack,
        GameObject newSource,
        SkillData newSkillData)
    {
        attack = newAttack;
        source = newSource;
        skillData = newSkillData;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (source != null &&
            (other.transform == source.transform ||
             other.transform.IsChildOf(source.transform)))
        {
            return;
        }

        HitReceiver receiver =
            other.GetComponentInParent<HitReceiver>();

        PlayerHitReceiver playerReceiver = null;

        if (receiver == null)
        {
            playerReceiver =
                other.GetComponentInParent<PlayerHitReceiver>();
        }

        if (receiver == null && playerReceiver == null)
            return;

        if (skillData == null)
        {
            Debug.LogWarning(
                $"{name} has no SkillData.",
                this
            );
            return;
        }

        if (!skillData.TryGetHitData(
                hitIndex,
                out SkillHitData skillHitData))
        {
            Debug.LogWarning(
                $"{name} has invalid hitIndex {hitIndex} " +
                $"for {skillData.name}.",
                this
            );
            return;
        }

        float baseDamage =
            attack * skillHitData.baseDamagePercent / 100f;

        // TEMP: rounding rule unresolved.
        int damage = Mathf.RoundToInt(baseDamage);

        HitData hitData = new HitData
        {
            damage = damage,
            hitIndex = hitIndex,
            source = source,
            hitStunDuration = skillHitData.hitStunDuration,
            knockbackPower = skillHitData.knockbackPower,
            knockbackMultiplier = skillHitData.knockbackMultiplier
        };

        if (receiver != null)
            receiver.ReceiveHit(hitData);
        else
            playerReceiver.ReceiveHit(hitData);
    }
}
