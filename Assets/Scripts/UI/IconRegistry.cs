using System;
using UnityEngine;

// Presentation keys deliberately do not replace the gameplay EquipmentSlot definition.
public enum EquipmentPartIcon { Belt, Bracelet, Necklace, Gloves, HairAccessory, BackAccessory, Earrings, Rings, Badge }
public enum CurrencyIcon { Gold, MemoryFragment }

[DisallowMultipleComponent]
public sealed class IconRegistry : MonoBehaviour
{
    [Serializable] public struct StatEntry { public StatType stat; public Sprite sprite; }
    [Serializable] public struct ElementEntry { public SkillElementType element; public Sprite sprite; }
    [Serializable] public struct PartEntry { public EquipmentPartIcon part; public Sprite sprite; }
    [Serializable] public struct CurrencyEntry { public CurrencyIcon currency; public Sprite sprite; }
    [Serializable] public struct SetEntry { public EquipmentSetData set; public Sprite sprite; }

    [Header("Stats (element stats reuse Elements unless overridden)")]
    [SerializeField] private StatEntry[] stats = Array.Empty<StatEntry>();
    [SerializeField] private Sprite staggerGauge;
    [Header("Elements (existing child overlays remain in the views)")]
    [SerializeField] private ElementEntry[] elements = Array.Empty<ElementEntry>();
    [Header("Equipment parts")]
    [SerializeField] private PartEntry[] equipmentParts = Array.Empty<PartEntry>();
    [Header("Currencies")]
    [SerializeField] private CurrencyEntry[] currencies = Array.Empty<CurrencyEntry>();
    [Header("Sets (no definition data is created here)")]
    [SerializeField] private SetEntry[] sets = Array.Empty<SetEntry>();

    public Sprite GetStatIcon(StatType stat)
    {
        foreach (var entry in stats ?? Array.Empty<StatEntry>())
            if (entry.stat == stat && entry.sprite != null) return entry.sprite;
        return GetElementIcon(GetStatElement(stat));
    }
    public Sprite GetStaggerGaugeIcon() => staggerGauge;
    public Sprite GetElementIcon(SkillElementType element)
    {
        if (element == SkillElementType.None) return null;
        foreach (var entry in elements ?? Array.Empty<ElementEntry>())
            if (entry.element == element && entry.sprite != null) return entry.sprite;
        return null;
    }
    public Sprite GetEquipmentPartIcon(EquipmentPartIcon part)
    {
        foreach (var entry in equipmentParts ?? Array.Empty<PartEntry>())
            if (entry.part == part && entry.sprite != null) return entry.sprite;
        return null;
    }
    public Sprite GetEquipmentPartIcon(EquipmentSlot slot)
        => EquipmentSlotUtility.IsValid(slot) && Enum.TryParse(slot.ToString(), out EquipmentPartIcon part)
            ? GetEquipmentPartIcon(part) : null;
    public Sprite GetEquipmentPartIcon(string part)
        => EquipmentSlotUtility.TryParse(part, out var slot) ? GetEquipmentPartIcon(slot) : null;
    public Sprite GetCurrencyIcon(CurrencyIcon currency)
    {
        foreach (var entry in currencies ?? Array.Empty<CurrencyEntry>())
            if (entry.currency == currency && entry.sprite != null) return entry.sprite;
        return null;
    }
    public Sprite GetSetIcon(EquipmentSetData set)
    {
        if (set == null) return null;
        foreach (var entry in sets ?? Array.Empty<SetEntry>())
            if (entry.set == set && entry.sprite != null) return entry.sprite;
        return null;
    }
    private static SkillElementType GetStatElement(StatType stat) => stat switch
    {
        StatType.FireEnhancement or StatType.FireResistance => SkillElementType.Fire,
        StatType.WaterEnhancement or StatType.WaterResistance => SkillElementType.Water,
        StatType.EarthEnhancement or StatType.EarthResistance => SkillElementType.Earth,
        StatType.WindEnhancement or StatType.WindResistance => SkillElementType.Wind,
        StatType.LightningEnhancement or StatType.LightningResistance => SkillElementType.Lightning,
        StatType.IceEnhancement or StatType.IceResistance => SkillElementType.Ice,
        StatType.LightEnhancement or StatType.LightResistance => SkillElementType.Light,
        StatType.DarkEnhancement or StatType.DarkResistance => SkillElementType.Dark,
        _ => SkillElementType.None
    };
}
