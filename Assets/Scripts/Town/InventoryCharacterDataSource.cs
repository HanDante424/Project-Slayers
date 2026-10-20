using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class InventoryCharacterDataSource : TownCharacterDataSource
{
    [Serializable] public struct CharacterIdentity { public PlayableCharacterData character; public string id; }
    [Tooltip("Stable persistence IDs; Town uses each existing character asset's GUID, not its display name.")]
    [SerializeField] private CharacterIdentity[] characters = Array.Empty<CharacterIdentity>();
    private readonly Dictionary<PlayableCharacterData, CharacterStats> stats = new(), without = new(), replacements = new();
    private readonly Dictionary<string, TownEquipmentItem> items = new();
    private CharacterEquipment equipment;
    private void Awake()
    {
        foreach (var entry in characters)
            if (!InventorySession.RegisterCharacter(entry.character, entry.id)) Debug.LogError("Invalid/duplicate character inventory identity: " + entry.id, this);
    }
    private void OnEnable() { equipment = InventorySession.Equipment; equipment.Changed += OnEquipmentChanged; }
    private void OnDisable() { if (equipment != null) equipment.Changed -= OnEquipmentChanged; }
    private void OnEquipmentChanged()
    {
        foreach (var id in items.Keys.ToArray()) if (InventorySession.Inventory.GetEquipment(id) == null) items.Remove(id);
        NotifyChanged();
    }
    private void OnDestroy()
    {
        foreach (var collection in new[] { stats, without, replacements })
            foreach (var value in collection.Values) if (value != null) Destroy(value.gameObject);
    }
    private CharacterStats Create(PlayableCharacterData character, int level, bool live)
    {
        var root = new GameObject("Equipment stats: " + character.name) { hideFlags = HideFlags.DontSave };
        root.SetActive(false); root.transform.SetParent(transform, false);
        var target = root.AddComponent<CharacterStats>(); target.Initialize(character.statsData, level);
        if (live)
        {
            root.AddComponent<CharacterEquipmentBinding>().Initialize(target, InventorySession.GetCharacterId(character));
            root.SetActive(true);
        }
        return target;
    }
    public override CharacterStats GetStats(PlayableCharacterData character)
    {
        if (character?.statsData == null || InventorySession.GetCharacterId(character) == null) return null;
        int level = TownCharacterSelection.Character == character ? TownCharacterSelection.Level : 1;
        if (!stats.TryGetValue(character, out var value) || value == null) stats[character] = value = Create(character, level, true);
        return value;
    }
    private TownEquipmentItem Wrap(EquipmentInstance instance)
    {
        if (instance == null) return null;
        if (!items.TryGetValue(instance.InstanceId, out var value)) items[instance.InstanceId] = value = new TownEquipmentItem(instance);
        return value;
    }
    public override TownEquipmentItem GetEquipped(PlayableCharacterData character, string slot)
        => EquipmentSlotUtility.TryParse(slot, out var key) ? Wrap(InventorySession.Equipment.GetEquipped(InventorySession.GetCharacterId(character), key)) : null;
    public override IReadOnlyList<TownEquipmentItem> GetOwned(PlayableCharacterData character, string slot)
        => InventorySession.GetCharacterId(character) != null && EquipmentSlotUtility.TryParse(slot, out var key)
            ? InventorySession.Inventory.Equipment.Where(i => i.Definition.slot == key).Select(Wrap).ToArray() : Array.Empty<TownEquipmentItem>();
    public override bool CanEquip(PlayableCharacterData character, string slot, TownEquipmentItem item)
        => EquipmentSlotUtility.TryParse(slot, out var key) && InventorySession.Equipment.CanEquip(InventorySession.GetCharacterId(character), key, item?.Instance);
    public override bool TryEquip(PlayableCharacterData character, string slot, TownEquipmentItem item)
        => EquipmentSlotUtility.TryParse(slot, out var key) && InventorySession.Equipment.Equip(InventorySession.GetCharacterId(character), key, item?.Instance);
    public override bool TryUnequip(PlayableCharacterData character, string slot)
        => EquipmentSlotUtility.TryParse(slot, out var key) && InventorySession.Equipment.Unequip(InventorySession.GetCharacterId(character), key);
    public override CharacterStats GetStatsWithoutEquipment(PlayableCharacterData character)
        => Snapshot(character, without);
    private CharacterStats Snapshot(PlayableCharacterData character, Dictionary<PlayableCharacterData, CharacterStats> cache)
    {
        var source = GetStats(character); if (source == null) return null;
        if (!cache.TryGetValue(character, out var target) || target == null) cache[character] = target = Create(character, source.Level, false);
        if (target.Level != source.Level) target.SetLevel(source.Level);
        target.CopyRuntimeModifiersFrom(source, EquipmentStatEvaluator.SourcePrefix);
        return target;
    }
    public override CharacterStats GetReplacementStats(PlayableCharacterData character, string slot, TownEquipmentItem item)
    {
        if (!CanEquip(character, slot, item)) return null;
        var target = Snapshot(character, replacements); if (target == null) return null;
        var equipped = InventorySession.Equipment.GetEquipped(InventorySession.GetCharacterId(character)).Where(i => i.Definition.slot.ToString() != slot).ToList();
        equipped.Add(item.Instance); EquipmentStatEvaluator.Apply(target, equipped); return target;
    }
    public override IReadOnlyList<TownEquipmentSet> GetSets(PlayableCharacterData character)
    {
        var equipped = InventorySession.Equipment.GetEquipped(InventorySession.GetCharacterId(character));
        var evaluated = EquipmentStatEvaluator.Evaluate(equipped);
        foreach (var owned in InventorySession.Inventory.Equipment)
        {
            if (owned.Definition.set != null && !evaluated.Sets.ContainsKey(owned.Definition.set)) evaluated.Sets.Add(owned.Definition.set, 0);
            foreach (var set in owned.AdditionalSets) if (set != null && !evaluated.Sets.ContainsKey(set)) evaluated.Sets.Add(set, 0);
        }
        return evaluated.Sets.Select(pair =>
        {
            var bonuses = pair.Key.bonuses?.Where(b => b != null).OrderBy(b => b.requiredEquipmentCount).ToArray() ?? Array.Empty<EquipmentSetBonus>();
            string effects = string.Join("\n", bonuses.Select(b => b.requiredEquipmentCount + "세트: " + string.Join(", ",
                (b.statModifiers ?? new List<EquipmentStatModifier>()).Where(m => m != null).Select(m => TownCharacterEquipmentController.StatName(m.statType) + " " + CharacterStatPanelView.FormatNumber(m.value) + (m.modifierType == StatModifierType.Percent ? "%" : ""))) +
                (b.additionalStaggerGaugePercent != 0 ? " 무력화 게이지 +" + b.additionalStaggerGaugePercent + "%" : "")));
            string members = string.Join("\n", InventorySession.Inventory.Equipment.Where(i => i.Definition.set == pair.Key || i.AdditionalSets.Contains(pair.Key)).Select(i => i.Definition.equipmentName).Distinct());
            return new TownEquipmentSet(pair.Key, pair.Key.setName, null, members, effects, pair.Value,
                bonuses.Any(b => b.requiredEquipmentCount >= 1 && pair.Value >= b.requiredEquipmentCount));
        }).ToArray();
    }
}
