using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterStatsData",
    menuName = "Slayers/Character Stats Data")]
public class CharacterStatsData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;

    [Header("Basic Stats")]
    public StatGrowth health = new StatGrowth(0f, 0f);
    public StatGrowth mana = new StatGrowth(0f, 0f);
    public StatGrowth attack = new StatGrowth(0f, 0f);
    public StatGrowth magic = new StatGrowth(0f, 0f);
    public StatGrowth defense = new StatGrowth(0f, 0f);
    public StatGrowth resistance = new StatGrowth(0f, 0f);

    [Header("Detailed Stats")]
    public StatGrowth attackSpeed = new StatGrowth(0f, 0f);
    public StatGrowth moveSpeed = new StatGrowth(0f, 0f);
    public StatGrowth statusResistance = new StatGrowth(0f, 0f);
    public StatGrowth luck = new StatGrowth(0f, 0f);
    public StatGrowth accuracy = new StatGrowth(0f, 0f);
    public StatGrowth evasion = new StatGrowth(0f, 0f);
    public StatGrowth recovery = new StatGrowth(0f, 0f);
    public StatGrowth cooldownReduction = new StatGrowth(0f, 0f);
    public StatGrowth awakening = new StatGrowth(0f, 0f);
    public StatGrowth stagger = new StatGrowth(0f, 0f);
    public StatGrowth criticalChance = new StatGrowth(0f, 0f);
    public StatGrowth criticalDamage = new StatGrowth(0f, 0f);
    public StatGrowth defensePenetration = new StatGrowth(0f, 0f);
    public StatGrowth resistancePenetration = new StatGrowth(0f, 0f);

    [Header("Derived Stats")]
    [Min(0f)]
    public float staggerGaugeHealthPercent = 40f;

    [Header("Element Enhancement")]
    public StatGrowth fireEnhancement = new StatGrowth(0f, 0f);
    public StatGrowth waterEnhancement = new StatGrowth(0f, 0f);
    public StatGrowth earthEnhancement = new StatGrowth(0f, 0f);
    public StatGrowth windEnhancement = new StatGrowth(0f, 0f);
    public StatGrowth lightningEnhancement = new StatGrowth(0f, 0f);
    public StatGrowth iceEnhancement = new StatGrowth(0f, 0f);
    public StatGrowth lightEnhancement = new StatGrowth(0f, 0f);
    public StatGrowth darkEnhancement = new StatGrowth(0f, 0f);

    [Header("Element Resistance")]
    public StatGrowth fireResistance = new StatGrowth(0f, 0f);
    public StatGrowth waterResistance = new StatGrowth(0f, 0f);
    public StatGrowth earthResistance = new StatGrowth(0f, 0f);
    public StatGrowth windResistance = new StatGrowth(0f, 0f);
    public StatGrowth lightningResistance = new StatGrowth(0f, 0f);
    public StatGrowth iceResistance = new StatGrowth(0f, 0f);
    public StatGrowth lightResistance = new StatGrowth(0f, 0f);
    public StatGrowth darkResistance = new StatGrowth(0f, 0f);

    private void OnValidate()
    {
        EnsureAndValidate(ref health);
        EnsureAndValidate(ref mana);
        EnsureAndValidate(ref attack);
        EnsureAndValidate(ref magic);
        EnsureAndValidate(ref defense);
        EnsureAndValidate(ref resistance);

        EnsureAndValidate(ref attackSpeed);
        EnsureAndValidate(ref moveSpeed);
        EnsureAndValidate(ref statusResistance);
        EnsureAndValidate(ref luck);
        EnsureAndValidate(ref accuracy);
        EnsureAndValidate(ref evasion);
        EnsureAndValidate(ref recovery);
        EnsureAndValidate(ref cooldownReduction);
        EnsureAndValidate(ref awakening);
        EnsureAndValidate(ref stagger);
        EnsureAndValidate(ref criticalChance);
        EnsureAndValidate(ref criticalDamage);
        EnsureAndValidate(ref defensePenetration);
        EnsureAndValidate(ref resistancePenetration);

        EnsureAndValidate(ref fireEnhancement);
        EnsureAndValidate(ref waterEnhancement);
        EnsureAndValidate(ref earthEnhancement);
        EnsureAndValidate(ref windEnhancement);
        EnsureAndValidate(ref lightningEnhancement);
        EnsureAndValidate(ref iceEnhancement);
        EnsureAndValidate(ref lightEnhancement);
        EnsureAndValidate(ref darkEnhancement);

        EnsureAndValidate(ref fireResistance);
        EnsureAndValidate(ref waterResistance);
        EnsureAndValidate(ref earthResistance);
        EnsureAndValidate(ref windResistance);
        EnsureAndValidate(ref lightningResistance);
        EnsureAndValidate(ref iceResistance);
        EnsureAndValidate(ref lightResistance);
        EnsureAndValidate(ref darkResistance);

        staggerGaugeHealthPercent =
            Mathf.Max(
                0f,
                Mathf.Round(staggerGaugeHealthPercent * 100f) / 100f
            );
    }

    private static void EnsureAndValidate(ref StatGrowth stat)
    {
        if (stat == null)
        {
            stat = new StatGrowth(0f, 0f);
        }

        stat.Validate();
    }
}