using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int hitIndex;

    private float attack;
    private GameObject source;
    private SkillData skillData;
    private int skillLevel = 1;

    public void Initialize(
        float newAttack,
        GameObject newSource,
        SkillData newSkillData,
        int newSkillLevel = 1)
    {
        attack = newAttack;
        source = newSource;
        skillData = newSkillData;
        skillLevel = newSkillData != null
            ? newSkillData.GetEffectiveLevel(newSkillLevel)
            : Mathf.Max(1, newSkillLevel);
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

        float damagePercent =
            skillHitData.baseDamagePercent +
            skillHitData.damageGrowthPerLevel * (skillLevel - 1);

        float rawDamage = attack * damagePercent / 100f;

        // TEMP: rounding rule unresolved.
        int damage = Mathf.RoundToInt(rawDamage);

        float staggerAmount =
            damage * skillHitData.staggerPercent / 100f;

        HitData hitData = new HitData
        {
            damage = damage,
            staggerAmount = staggerAmount,
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
