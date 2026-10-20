using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// In-memory session only. No automatic grants, SO mutation or disk writes.
public static class InventorySession
{
    public static SharedInventory Inventory { get; private set; } = new();
    public static CharacterEquipment Equipment { get; private set; } = new(Inventory);
    private static readonly Dictionary<PlayableCharacterData, string> characterIds = new();
    public static bool RegisterCharacter(PlayableCharacterData character, string stableId)
    {
        if (character == null || string.IsNullOrWhiteSpace(stableId) ||
            (characterIds.TryGetValue(character, out var existing) && existing != stableId) ||
            characterIds.Any(p => p.Key != character && p.Value == stableId)) return false;
        characterIds[character] = stableId; return true;
    }
    public static string GetCharacterId(PlayableCharacterData character)
        => character != null && characterIds.TryGetValue(character, out var id) ? id : null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    { Inventory = new SharedInventory(); Equipment = new CharacterEquipment(Inventory); characterIds.Clear(); }

    // The persistence layer supplies stable set IDs (e.g. asset GUIDs), not names or serialized SO copies.
    public static InventoryRecord Capture(Func<EquipmentSetData, string> setId)
    {
        if (setId == null) throw new ArgumentNullException(nameof(setId));
        return new InventoryRecord
        {
            gold = Inventory.Gold, memoryFragments = Inventory.MemoryFragments,
            items = Inventory.Items.Select(i => new ItemOwnershipRecord { itemCode = i.Definition.itemCode, quantity = i.Quantity }).ToList(),
            equipment = Inventory.Equipment.Select(i => new EquipmentInstanceRecord
            {
                instanceId = i.InstanceId, equipmentCode = i.Definition.equipmentCode, enhancementLevel = i.EnhancementLevel,
                additionalModifiers = i.AdditionalModifiers.Select(m => new EquipmentModifierRecord { stat = m.StatType, type = m.ModifierType, value = m.Value }).ToList(),
                additionalSetIds = i.AdditionalSets.Select(s =>
                {
                    var id = setId(s);
                    if (string.IsNullOrEmpty(id)) throw new InvalidOperationException("Additional set has no persistence ID.");
                    return id;
                }).ToList()
            }).ToList(),
            equipped = Equipment.Capture().ToList()
        };
    }
}

[Serializable] public sealed class InventoryRecord
{
    public long gold, memoryFragments;
    public List<ItemOwnershipRecord> items = new();
    public List<EquipmentInstanceRecord> equipment = new();
    public List<CharacterEquipmentRecord> equipped = new();
}
[Serializable] public sealed class ItemOwnershipRecord { public int itemCode; public long quantity; }
[Serializable] public sealed class EquipmentModifierRecord { public StatType stat; public StatModifierType type; public float value; }
[Serializable] public sealed class EquipmentInstanceRecord
{
    public string instanceId;
    public int equipmentCode, enhancementLevel;
    public List<EquipmentModifierRecord> additionalModifiers = new();
    public List<string> additionalSetIds = new();
}
[Serializable] public sealed class CharacterEquipmentRecord { public string characterId; public EquipmentSlot slot; public string instanceId; }
