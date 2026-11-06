using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private int hitIndex;

    private float attack;
    private GameObject source;
    private SkillData skillData;
    private int skillLevel = 1;
    private bool initialized;
    private CharacterStats sourceStats;
    private AttackHitboxController effectController;

    public void Initialize(
        float newAttack,
        GameObject newSource,
        SkillData newSkillData,
        int newSkillLevel = 1)
    {
        initialized = true;
        sourceStats = null;
        effectController = null;
        attack = newAttack;
        source = newSource;
        skillData = newSkillData;
        skillLevel = newSkillData != null
            ? newSkillData.GetEffectiveLevel(newSkillLevel)
            : Mathf.Max(1, newSkillLevel);
    }

    internal void InitializeEffect(CharacterStats stats, GameObject owner, EffectAttackSettings settings,
        AttackHitboxController controller)
    {
        settings.skill ??= new LearnedSkill();
        settings.skill.Bind(settings.skillData);
        Initialize(stats != null ? stats.FinalAttack : 0f, owner, settings.skillData, settings.skill.CurrentLevel);
        sourceStats = stats;
        hitIndex = settings.hitIndex;
        effectController = controller;
    }

    internal void ClearEffectInitialization()
    {
        initialized = false;
        source = null;
        sourceStats = null;
        skillData = null;
        effectController = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryContact(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Effect windows may open while already overlapping. Legacy animation-event windows stay unchanged.
        if (effectController != null)
            TryContact(other);
    }

    private void TryContact(Collider2D other)
    {
        if (!initialized || other == null || !isActiveAndEnabled)
            return;
        if (effectController != null && !effectController.CanProcessEffectContact())
            return;
        if (source != null &&
            (other.transform == source.transform ||
             other.transform.IsChildOf(source.transform)))
        {
            return;
        }

        // Ignore attack volumes, including a following effect parented under another actor.
        if (effectController != null && other.GetComponentInParent<AttackHitbox>() != null)
            return;

        HitReceiver receiver =
            other.GetComponentInParent<HitReceiver>();

        if (effectController != null)
        {
            // Existing project identity: Mayhem targets enemy HitReceiver, never player/ally receivers.
            if (receiver == null || receiver.IsDead || other.GetComponentInParent<PlayerHitReceiver>() != null ||
                (source != null && source.GetComponentInParent<HitReceiver>() != null))
                return;
            if (!effectController.IsDamageReady || source == null || sourceStats == null)
                return;
        }

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

        // Invalid data must not consume this effect instance's one hit per target.
        if (effectController != null && !effectController.TryRegisterEffectTarget(receiver))
            return;

        float damagePercent =
            skillHitData.baseDamagePercent +
            skillHitData.damageGrowthPerLevel * (skillLevel - 1);

        // Effect hits read current final attack at impact; the legacy snapshot overload remains compatible.
        float currentAttack = sourceStats != null ? sourceStats.FinalAttack : attack;
        float rawDamage = currentAttack * damagePercent / 100f;

        // Final damage truncates decimals (Docs/02_StatsAndDamage.md 13-1).
        int damage = Mathf.FloorToInt(rawDamage);

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
