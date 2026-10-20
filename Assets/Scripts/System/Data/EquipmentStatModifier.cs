using System;
using System.Collections.Generic;

[Serializable]
public class EquipmentStatModifier
{
    public StatType statType;
    public StatModifierType modifierType;
    public float value;

    // Expand an all-element option into the existing serialized modifier lists.
    // Callers explicitly add these entries; no automatic SO mutation or runtime application.
    public static List<EquipmentStatModifier> CreateAllElementEnhancements(StatModifierType type, float value)
        => CreateModifiers(type, value, StatType.FireEnhancement, StatType.WaterEnhancement,
            StatType.EarthEnhancement, StatType.WindEnhancement, StatType.LightningEnhancement,
            StatType.IceEnhancement, StatType.LightEnhancement, StatType.DarkEnhancement);

    public static List<EquipmentStatModifier> CreateAllElementResistances(StatModifierType type, float value)
        => CreateModifiers(type, value, StatType.FireResistance, StatType.WaterResistance,
            StatType.EarthResistance, StatType.WindResistance, StatType.LightningResistance,
            StatType.IceResistance, StatType.LightResistance, StatType.DarkResistance);

    private static List<EquipmentStatModifier> CreateModifiers(StatModifierType type, float value, params StatType[] stats)
    {
        var result = new List<EquipmentStatModifier>(stats.Length);
        foreach (var stat in stats)
            result.Add(new EquipmentStatModifier { statType = stat, modifierType = type, value = value });
        return result;
    }

    // Pure conversion for a future equipment evaluator; CharacterStats is unchanged.
    public static float CalculateStaggerGaugeHealthPercent(float basePercent, float additionalPercent)
        => basePercent * (1f + additionalPercent / 100f);
}
