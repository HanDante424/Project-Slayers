using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TownCharacterOverviewView : MonoBehaviour
{
    [SerializeField] private IconRegistry iconRegistry;
    [Serializable] public struct ElementSlot { public TMP_Text text; public Image icon; }
    [Header("Element sprites (assigned by the user)")]
    [SerializeField] private Sprite fire, water, earth, wind, lightning, ice;
    [SerializeField, InspectorName("Light")] private Sprite lightSprite;
    [SerializeField] private Sprite dark;
    [SerializeField] private ElementSlot[] enhancementSlots;
    [SerializeField] private ElementSlot[] resistanceSlots;
    private CharacterStats source;
    private CharacterStats subscribedSource;
    private static readonly string[] Names = { "화염", "물", "대지", "바람", "번개", "얼음", "빛", "어둠" };
    private static readonly StatType[] Power = { StatType.FireEnhancement, StatType.WaterEnhancement, StatType.EarthEnhancement, StatType.WindEnhancement, StatType.LightningEnhancement, StatType.IceEnhancement, StatType.LightEnhancement, StatType.DarkEnhancement };
    private static readonly StatType[] Resist = { StatType.FireResistance, StatType.WaterResistance, StatType.EarthResistance, StatType.WindResistance, StatType.LightningResistance, StatType.IceResistance, StatType.LightResistance, StatType.DarkResistance };
    public static int[] GetTopElements(CharacterStats stats, bool resistance) => stats == null || stats.Data == null ? Array.Empty<int>() :
        Enumerable.Range(0, 8).OrderByDescending(i => stats.GetFinalStat((resistance ? Resist : Power)[i])).ThenBy(i => i).Take(2).ToArray();
    public void Bind(CharacterStats stats)
    {
        Unsubscribe();
        source = stats;
        Subscribe();
        Refresh();
    }
    private void OnEnable() { Subscribe(); Refresh(); }
    private void OnDisable() => Unsubscribe();
    private void OnDestroy() => Unsubscribe();
    private void Subscribe()
    {
        if (!isActiveAndEnabled || source == null || subscribedSource != null) return;
        subscribedSource = source; subscribedSource.StatsChanged += Refresh;
    }
    private void Unsubscribe()
    {
        if (subscribedSource != null) subscribedSource.StatsChanged -= Refresh;
        subscribedSource = null;
    }
    public void Refresh() { Show(enhancementSlots, false); Show(resistanceSlots, true); }
    private void Show(ElementSlot[] slots, bool resistance)
    {
        if (slots == null) return;
        var order = GetTopElements(source, resistance);
        var sprites = new[] { fire, water, earth, wind, lightning, ice, lightSprite, dark };
        for (int i = 0; i < slots.Length; i++)
        {
            bool valid = i < order.Length; int index = valid ? order[i] : 0;
            if (slots[i].text != null) slots[i].text.text = valid ? Names[index] + " " + CharacterStatPanelView.FormatStat((resistance ? Resist : Power)[index], source.GetFinalStat((resistance ? Resist : Power)[index])) : "";
            if (slots[i].icon != null) { slots[i].icon.sprite = valid ? (iconRegistry != null ? iconRegistry.GetElementIcon((SkillElementType)(index + 1)) : null) ?? sprites[index] : null; slots[i].icon.enabled = slots[i].icon.sprite != null; }
        }
    }
}
