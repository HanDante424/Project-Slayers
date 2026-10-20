using System;
using System.Collections.Generic;
using System.Linq;

// One instance may be referenced by any number of characters. Slots belong to characters, not items.
public sealed class CharacterEquipment
{
    private readonly SharedInventory inventory;
    private readonly Dictionary<string, Dictionary<EquipmentSlot, string>> loadouts = new(StringComparer.Ordinal);
    public event Action Changed;
    public CharacterEquipment(SharedInventory inventory) { this.inventory = inventory; inventory.Changed += OnInventoryChanged; }
    public EquipmentInstance GetEquipped(string characterId, EquipmentSlot slot)
        => characterId != null && loadouts.TryGetValue(characterId, out var slots) && slots.TryGetValue(slot, out var id) ? inventory.GetEquipment(id) : null;
    public IReadOnlyList<EquipmentInstance> GetEquipped(string characterId)
        => characterId != null && loadouts.TryGetValue(characterId, out var slots)
            ? slots.Values.Select(inventory.GetEquipment).Where(i => i != null).ToArray() : Array.Empty<EquipmentInstance>();
    public bool CanEquip(string characterId, EquipmentSlot slot, EquipmentInstance instance)
        => !string.IsNullOrEmpty(characterId) && EquipmentSlotUtility.IsValid(slot) && instance?.Definition != null &&
           instance.Definition.slot == slot && ReferenceEquals(inventory.GetEquipment(instance.InstanceId), instance);
    public bool Equip(string characterId, EquipmentSlot slot, EquipmentInstance instance)
    {
        if (!CanEquip(characterId, slot, instance)) return false;
        if (!loadouts.TryGetValue(characterId, out var slots)) loadouts.Add(characterId, slots = new());
        if (slots.TryGetValue(slot, out var old) && old == instance.InstanceId) return true;
        slots[slot] = instance.InstanceId; Changed?.Invoke(); return true;
    }
    public bool Unequip(string characterId, EquipmentSlot slot)
    {
        if (characterId == null || !loadouts.TryGetValue(characterId, out var slots) || !slots.Remove(slot)) return false;
        Changed?.Invoke(); return true;
    }
    private void OnInventoryChanged()
    {
        foreach (var slots in loadouts.Values)
            foreach (var slot in slots.Keys.ToArray())
                if (inventory.GetEquipment(slots[slot]) == null) slots.Remove(slot);
        // Also propagates instance option/upgrade changes to every wearer.
        Changed?.Invoke();
    }
    public IReadOnlyList<CharacterEquipmentRecord> Capture()
        => loadouts.SelectMany(c => c.Value.Select(s => new CharacterEquipmentRecord { characterId = c.Key, slot = s.Key, instanceId = s.Value })).ToArray();
}
