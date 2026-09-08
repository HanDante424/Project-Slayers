using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private CharacterStatsData characterData;

    [Header("Level")]
    [SerializeField]
    [Min(1)]
    private int level = 1;

    private readonly List<StatModifier> modifiers = new();
    private readonly Dictionary<StatType, float> finalStats = new();
    private readonly Dictionary<StatType, float> flatTotals = new();
    private readonly Dictionary<StatType, float> percentTotals = new();

    public int Level => level;
    public CharacterStatsData Data => characterData;
    public int ModifierCount => modifiers.Count;


    // ==================================================
    // Current Level Stats
    // 현재는 CharacterStatsData의 레벨 성장값
    // ==================================================

    // Basic Stats
    public float MaxHealth =>
        characterData.health.GetValue(level);

    public float MaxMana =>
        characterData.mana.GetValue(level);

    public float Attack =>
        characterData.attack.GetValue(level);

    public float Magic =>
        characterData.magic.GetValue(level);

    public float Defense =>
        characterData.defense.GetValue(level);

    public float Resistance =>
        characterData.resistance.GetValue(level);


    // Detailed Stats
    public float AttackSpeed =>
        characterData.attackSpeed.GetValue(level);

    public float MoveSpeed =>
        characterData.moveSpeed.GetValue(level);

    public float StatusResistance =>
        characterData.statusResistance.GetValue(level);

    public float Luck =>
        characterData.luck.GetValue(level);

    public float Accuracy =>
        characterData.accuracy.GetValue(level);

    public float Evasion =>
        characterData.evasion.GetValue(level);

    public float Recovery =>
        characterData.recovery.GetValue(level);

    public float CooldownReduction =>
        characterData.cooldownReduction.GetValue(level);

    public float Awakening =>
        characterData.awakening.GetValue(level);

    public float Stagger =>
        characterData.stagger.GetValue(level);

    public float CriticalChance =>
        characterData.criticalChance.GetValue(level);

    public float CriticalDamage =>
        characterData.criticalDamage.GetValue(level);

    public float DefensePenetration =>
        characterData.defensePenetration.GetValue(level);

    public float ResistancePenetration =>
        characterData.resistancePenetration.GetValue(level);


    // Derived Stats
    public float StaggerGauge =>
        MaxHealth *
        (characterData.staggerGaugeHealthPercent / 100f);


    // Element Enhancement
    public float FireEnhancement =>
        characterData.fireEnhancement.GetValue(level);

    public float WaterEnhancement =>
        characterData.waterEnhancement.GetValue(level);

    public float EarthEnhancement =>
        characterData.earthEnhancement.GetValue(level);

    public float WindEnhancement =>
        characterData.windEnhancement.GetValue(level);

    public float LightningEnhancement =>
        characterData.lightningEnhancement.GetValue(level);

    public float IceEnhancement =>
        characterData.iceEnhancement.GetValue(level);

    public float LightEnhancement =>
        characterData.lightEnhancement.GetValue(level);

    public float DarkEnhancement =>
        characterData.darkEnhancement.GetValue(level);


    // Element Resistance
    public float FireResistance =>
        characterData.fireResistance.GetValue(level);

    public float WaterResistance =>
        characterData.waterResistance.GetValue(level);

    public float EarthResistance =>
        characterData.earthResistance.GetValue(level);

    public float WindResistance =>
        characterData.windResistance.GetValue(level);

    public float LightningResistance =>
        characterData.lightningResistance.GetValue(level);

    public float IceResistance =>
        characterData.iceResistance.GetValue(level);

    public float LightResistance =>
        characterData.lightResistance.GetValue(level);

    public float DarkResistance =>
        characterData.darkResistance.GetValue(level);


    // ==================================================
    // Final Stats
    //
    // 장비 / 버프 / 디버프가 같은 Modifier 경로를 사용한다.
    // ==================================================

    public float FinalMaxHealth => GetFinalStat(StatType.Health);
    public float FinalMaxMana => GetFinalStat(StatType.Mana);

    public float FinalAttack => GetFinalStat(StatType.Attack);
    public float FinalMagic => GetFinalStat(StatType.Magic);
    public float FinalDefense => GetFinalStat(StatType.Defense);
    public float FinalResistance => GetFinalStat(StatType.Resistance);

    public float FinalAttackSpeed => GetFinalStat(StatType.AttackSpeed);
    public float FinalMoveSpeed => GetFinalStat(StatType.MoveSpeed);
    public float FinalStatusResistance => GetFinalStat(StatType.StatusResistance);

    public float FinalLuck => GetFinalStat(StatType.Luck);
    public float FinalAccuracy => GetFinalStat(StatType.Accuracy);
    public float FinalEvasion => GetFinalStat(StatType.Evasion);
    public float FinalRecovery => GetFinalStat(StatType.Recovery);

    public float FinalCooldownReduction => GetFinalStat(StatType.CooldownReduction);
    public float FinalAwakening => GetFinalStat(StatType.Awakening);
    public float FinalStagger => GetFinalStat(StatType.Stagger);

    public float FinalCriticalChance => GetFinalStat(StatType.CriticalChance);
    public float FinalCriticalDamage => GetFinalStat(StatType.CriticalDamage);

    public float FinalDefensePenetration => GetFinalStat(StatType.DefensePenetration);
    public float FinalResistancePenetration => GetFinalStat(StatType.ResistancePenetration);


    // Final Derived Stat
    public float FinalStaggerGauge =>
        FinalMaxHealth *
        (characterData.staggerGaugeHealthPercent / 100f);


    // Final Element Enhancement
    public float FinalFireEnhancement => GetFinalStat(StatType.FireEnhancement);
    public float FinalWaterEnhancement => GetFinalStat(StatType.WaterEnhancement);
    public float FinalEarthEnhancement => GetFinalStat(StatType.EarthEnhancement);
    public float FinalWindEnhancement => GetFinalStat(StatType.WindEnhancement);
    public float FinalLightningEnhancement => GetFinalStat(StatType.LightningEnhancement);
    public float FinalIceEnhancement => GetFinalStat(StatType.IceEnhancement);
    public float FinalLightEnhancement => GetFinalStat(StatType.LightEnhancement);
    public float FinalDarkEnhancement => GetFinalStat(StatType.DarkEnhancement);


    // Final Element Resistance
    public float FinalFireResistance => GetFinalStat(StatType.FireResistance);
    public float FinalWaterResistance => GetFinalStat(StatType.WaterResistance);
    public float FinalEarthResistance => GetFinalStat(StatType.EarthResistance);
    public float FinalWindResistance => GetFinalStat(StatType.WindResistance);
    public float FinalLightningResistance => GetFinalStat(StatType.LightningResistance);
    public float FinalIceResistance => GetFinalStat(StatType.IceResistance);
    public float FinalLightResistance => GetFinalStat(StatType.LightResistance);
    public float FinalDarkResistance => GetFinalStat(StatType.DarkResistance);


    private void Awake()
    {
        RecalculateFinalStats();
    }


    public void AddModifier(StatModifier modifier)
    {
        if (modifier == null)
            throw new ArgumentNullException(nameof(modifier));

        modifiers.Add(modifier);
        RecalculateFinalStats();
    }


    public bool RemoveModifier(StatModifier modifier)
    {
        if (!modifiers.Remove(modifier))
            return false;

        RecalculateFinalStats();
        return true;
    }


    public int RemoveModifiersBySource(string sourceId)
    {
        int removedCount = modifiers.RemoveAll(
            modifier => string.Equals(
                modifier.SourceId,
                sourceId,
                StringComparison.Ordinal
            )
        );

        if (removedCount > 0)
        {
            RecalculateFinalStats();
        }

        return removedCount;
    }


    public void ClearModifiers()
    {
        if (modifiers.Count == 0)
            return;

        modifiers.Clear();
        RecalculateFinalStats();
    }


    public void RecalculateFinalStats()
    {
        finalStats.Clear();
        flatTotals.Clear();
        percentTotals.Clear();

        if (characterData == null)
            return;

        foreach (StatModifier modifier in modifiers)
        {
            Dictionary<StatType, float> totals = modifier.ModifierType switch
            {
                StatModifierType.Flat => flatTotals,
                StatModifierType.Percent => percentTotals,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(modifier.ModifierType),
                    modifier.ModifierType,
                    null
                )
            };

            totals.TryGetValue(modifier.StatType, out float currentTotal);
            totals[modifier.StatType] = currentTotal + modifier.Value;
        }

        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            float currentLevelStat = GetCurrentLevelStat(statType);

            flatTotals.TryGetValue(statType, out float flatTotal);
            percentTotals.TryGetValue(statType, out float percentTotal);

            finalStats[statType] =
                (currentLevelStat + flatTotal) *
                (1f + percentTotal / 100f);
        }
    }


    public void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);
        RecalculateFinalStats();
    }


    private float GetFinalStat(StatType statType)
    {
        if (!finalStats.TryGetValue(statType, out float value))
        {
            RecalculateFinalStats();
            finalStats.TryGetValue(statType, out value);
        }

        return value;
    }


    private float GetCurrentLevelStat(StatType statType)
    {
        return statType switch
        {
            StatType.Health => MaxHealth,
            StatType.Mana => MaxMana,
            StatType.Attack => Attack,
            StatType.Magic => Magic,
            StatType.Defense => Defense,
            StatType.Resistance => Resistance,
            StatType.AttackSpeed => AttackSpeed,
            StatType.MoveSpeed => MoveSpeed,
            StatType.StatusResistance => StatusResistance,
            StatType.Luck => Luck,
            StatType.Accuracy => Accuracy,
            StatType.Evasion => Evasion,
            StatType.Recovery => Recovery,
            StatType.CooldownReduction => CooldownReduction,
            StatType.Awakening => Awakening,
            StatType.Stagger => Stagger,
            StatType.CriticalChance => CriticalChance,
            StatType.CriticalDamage => CriticalDamage,
            StatType.DefensePenetration => DefensePenetration,
            StatType.ResistancePenetration => ResistancePenetration,
            StatType.FireEnhancement => FireEnhancement,
            StatType.WaterEnhancement => WaterEnhancement,
            StatType.EarthEnhancement => EarthEnhancement,
            StatType.WindEnhancement => WindEnhancement,
            StatType.LightningEnhancement => LightningEnhancement,
            StatType.IceEnhancement => IceEnhancement,
            StatType.LightEnhancement => LightEnhancement,
            StatType.DarkEnhancement => DarkEnhancement,
            StatType.FireResistance => FireResistance,
            StatType.WaterResistance => WaterResistance,
            StatType.EarthResistance => EarthResistance,
            StatType.WindResistance => WindResistance,
            StatType.LightningResistance => LightningResistance,
            StatType.IceResistance => IceResistance,
            StatType.LightResistance => LightResistance,
            StatType.DarkResistance => DarkResistance,
            _ => throw new ArgumentOutOfRangeException(nameof(statType), statType, null)
        };
    }


    private void OnValidate()
    {
        level = Mathf.Max(1, level);
        RecalculateFinalStats();
    }
}
