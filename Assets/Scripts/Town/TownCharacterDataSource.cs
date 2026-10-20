using System;
using System.Collections.Generic;
using UnityEngine;

// Optional adapter to an actual inventory/progression owner. No state is stored in definition SOs.
// Slot keys match EquipmentSlot names; string virtual methods remain compatible with existing adapters.
public abstract class TownCharacterDataSource : MonoBehaviour
{
    public event Action Changed;
    protected void NotifyChanged() => Changed?.Invoke();
    public virtual CharacterStats GetStats(PlayableCharacterData character) => null;
    public virtual LearnedSkill GetLearnedSkill(PlayableCharacterData character, SkillData skill) => null;
    public virtual TownEquipmentItem GetEquipped(PlayableCharacterData character, string slot) => null;
    // Return stable item identities so repeated reads can validate a selected owned instance.
    public virtual IReadOnlyList<TownEquipmentItem> GetOwned(PlayableCharacterData character, string slot) => Array.Empty<TownEquipmentItem>();
    public virtual IReadOnlyList<TownEquipmentSet> GetSets(PlayableCharacterData character) => Array.Empty<TownEquipmentSet>();
    public virtual bool CanEquip(PlayableCharacterData character, string slot, TownEquipmentItem item) => false;
    public virtual bool TryEquip(PlayableCharacterData character, string slot, TownEquipmentItem item) => false;
    // Both snapshots must retain identical non-equipment modifiers. Replacement must include set gains/losses.
    // Return null while the inventory/set evaluator cannot provide an authoritative result.
    public virtual CharacterStats GetStatsWithoutEquipment(PlayableCharacterData character) => null;
    public virtual CharacterStats GetReplacementStats(PlayableCharacterData character, string slot, TownEquipmentItem item) => null;
}

public sealed class TownEquipmentItem
{
    public EquipmentData Definition { get; }
    public string UpgradeText { get; }
    public TownEquipmentItem(EquipmentData definition, string upgradeText = "") { Definition = definition; UpgradeText = upgradeText; }
}

// Presentation data supplied by the future set evaluator, not invented conditions on EquipmentSetData.
public sealed class TownEquipmentSet
{
    public EquipmentSetData Definition { get; }
    public string Name { get; }
    public Sprite Icon { get; }
    public string Members { get; }
    public string Effects { get; }
    public int EquippedCount { get; }
    public bool Active { get; }
    public TownEquipmentSet(EquipmentSetData definition, string name, Sprite icon, string members, string effects, int equippedCount, bool active)
    { Definition = definition; Name = name; Icon = icon; Members = members; Effects = effects; EquippedCount = equippedCount; Active = active; }
}
