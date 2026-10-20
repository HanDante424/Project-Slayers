using System;
using System.Collections.Generic;

public sealed class EquipmentEvaluation
{
    public readonly List<StatModifier> Modifiers = new();
    public readonly Dictionary<EquipmentSetData, int> Sets = new();
    public float AdditionalStaggerGaugePercent;
}

public static class EquipmentStatEvaluator
{
    public const string SourcePrefix = "equipment/";
    public static EquipmentEvaluation Evaluate(IEnumerable<EquipmentInstance> instances)
    {
        var result = new EquipmentEvaluation();
        foreach (var instance in instances)
        {
            var definition = instance?.Definition; if (definition == null) continue;
            string source = SourcePrefix + "instance/" + instance.InstanceId;
            Append(result, definition.baseStatModifiers, source);
            Append(result, definition.detailStatModifiers, source);
            Append(result, definition.elementEnhancementModifiers, source);
            Append(result, definition.elementResistanceModifiers, source);
            foreach (var modifier in instance.AdditionalModifiers)
                result.Modifiers.Add(new StatModifier(modifier.StatType, modifier.ModifierType, modifier.Value, source));
            result.AdditionalStaggerGaugePercent += definition.additionalStaggerGaugePercent;
            var contributions = new HashSet<EquipmentSetData>();
            if (definition.set != null) contributions.Add(definition.set);
            foreach (var set in instance.AdditionalSets) if (set != null) contributions.Add(set);
            foreach (var set in contributions)
            {
                result.Sets.TryGetValue(set, out int count); result.Sets[set] = count + 1;
            }
        }
        foreach (var pair in result.Sets)
        {
            if (pair.Key.bonuses == null) continue;
            foreach (var bonus in pair.Key.bonuses)
            {
                if (bonus == null || bonus.requiredEquipmentCount < 1 || pair.Value < bonus.requiredEquipmentCount) continue;
                Append(result, bonus.statModifiers, SourcePrefix + "sets");
                result.AdditionalStaggerGaugePercent += bonus.additionalStaggerGaugePercent;
            }
        }
        return result;
    }

    private static void Append(EquipmentEvaluation result, IEnumerable<EquipmentStatModifier> entries, string source)
    {
        if (entries == null) return;
        foreach (var entry in entries)
        {
            if (entry == null || !Enum.IsDefined(typeof(StatType), entry.statType) ||
                !Enum.IsDefined(typeof(StatModifierType), entry.modifierType) || float.IsNaN(entry.value) || float.IsInfinity(entry.value))
                throw new InvalidOperationException("Invalid equipment modifier definition.");
            result.Modifiers.Add(new StatModifier(entry.statType, entry.modifierType, entry.value, source));
        }
    }
    public static void Apply(CharacterStats stats, IEnumerable<EquipmentInstance> instances)
    {
        var result = Evaluate(instances);
        stats.ReplaceOwnedModifiers(SourcePrefix, result.Modifiers, result.AdditionalStaggerGaugePercent);
    }
}
