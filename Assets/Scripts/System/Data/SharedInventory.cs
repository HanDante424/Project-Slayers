using System;
using System.Collections.Generic;
using System.Linq;

public sealed class OwnedItem
{
    public ItemData Definition { get; }
    public long Quantity { get; }
    public OwnedItem(ItemData definition, long quantity) { Definition = definition; Quantity = quantity; }
}

// Definition registry and ownership deliberately have separate responsibilities.
public sealed class SharedInventory
{
    private Dictionary<int, OwnedItem> items = new();
    private Dictionary<string, EquipmentInstance> equipment = new(StringComparer.Ordinal);
    public long Gold { get; private set; }
    public long MemoryFragments { get; private set; }
    public event Action Changed;
    public IReadOnlyList<OwnedItem> Items => items.Values.OrderBy(i => i.Definition.itemCode).ToArray();
    public IReadOnlyList<EquipmentInstance> Equipment => equipment.Values.ToArray();
    public EquipmentInstance GetEquipment(string id) => id != null && equipment.TryGetValue(id, out var value) ? value : null;
    public long GetCount(ItemData item) => item != null && items.TryGetValue(item.itemCode, out var entry) && entry.Definition == item ? entry.Quantity : 0;

    public bool AddItem(ItemData item, long quantity)
    {
        if (item == null || item.itemCode < 0 || quantity <= 0 ||
            (items.TryGetValue(item.itemCode, out var entry) && entry.Definition != item)) return false;
        long total;
        try { total = checked(GetCount(item) + quantity); } catch (OverflowException) { return false; }
        items[item.itemCode] = new OwnedItem(item, total);
        Changed?.Invoke(); return true;
    }
    public bool RemoveItem(ItemData item, long quantity)
    {
        if (item == null || quantity <= 0 || GetCount(item) < quantity) return false;
        long remaining = GetCount(item) - quantity;
        if (remaining == 0) items.Remove(item.itemCode); else items[item.itemCode] = new OwnedItem(item, remaining);
        Changed?.Invoke(); return true;
    }
    public bool AddCurrency(long gold, long memoryFragments)
    {
        if (gold < 0 || memoryFragments < 0) return false;
        long nextGold, nextMemory;
        try { nextGold = checked(Gold + gold); nextMemory = checked(MemoryFragments + memoryFragments); }
        catch (OverflowException) { return false; }
        Gold = nextGold; MemoryFragments = nextMemory; Changed?.Invoke(); return true;
    }
    public bool SpendCurrency(long gold, long memoryFragments)
    {
        if (gold < 0 || memoryFragments < 0 || Gold < gold || MemoryFragments < memoryFragments) return false;
        Gold -= gold; MemoryFragments -= memoryFragments; Changed?.Invoke(); return true;
    }
    public bool RegisterEquipment(EquipmentInstance instance)
    {
        if (instance?.Definition == null || string.IsNullOrEmpty(instance.InstanceId) || equipment.ContainsKey(instance.InstanceId)) return false;
        equipment.Add(instance.InstanceId, instance); instance.Changed += OnInstanceChanged;
        Changed?.Invoke(); return true;
    }
    public bool RemoveEquipment(string id)
    {
        var instance = GetEquipment(id); if (instance == null) return false;
        equipment.Remove(id); instance.Changed -= OnInstanceChanged;
        Changed?.Invoke(); return true;
    }
    private void OnInstanceChanged() => Changed?.Invoke();

    public bool CanCraft(EquipmentData definition, int quantity) => TryGetCost(definition, quantity, out _, out _, out _);
    private bool TryGetCost(EquipmentData definition, int quantity, out long gold, out long memory, out Dictionary<ItemData, long> materials)
    {
        gold = memory = 0; materials = new();
        if (definition == null || definition.equipmentCode < 0 || !EquipmentSlotUtility.IsValid(definition.slot) || quantity <= 0 ||
            definition.craftingGold < 0 || definition.craftingMemoryFragments < 0 || definition.craftingMaterials == null) return false;
        // All-empty definitions are not silently treated as free recipes.
        if (definition.craftingGold == 0 && definition.craftingMemoryFragments == 0 && definition.craftingMaterials.Count == 0) return false;
        try
        {
            gold = checked((long)definition.craftingGold * quantity);
            memory = checked((long)definition.craftingMemoryFragments * quantity);
            _ = checked(equipment.Count + quantity);
            foreach (var requirement in definition.craftingMaterials)
            {
                if (requirement?.material == null || requirement.material.itemCode < 0 || requirement.quantity <= 0) return false;
                materials.TryGetValue(requirement.material, out long prior);
                materials[requirement.material] = checked(prior + checked((long)requirement.quantity * quantity));
            }
        }
        catch (OverflowException) { return false; }
        return Gold >= gold && MemoryFragments >= memory && materials.All(m => GetCount(m.Key) >= m.Value);
    }
    public bool TryCraft(EquipmentData definition, int quantity)
    {
        if (!TryGetCost(definition, quantity, out long gold, out long memory, out var materials)) return false;
        // Prepare the entire next state before changing wallet, materials or ownership.
        var nextItems = new Dictionary<int, OwnedItem>(items);
        foreach (var material in materials)
        {
            long remaining = GetCount(material.Key) - material.Value;
            if (remaining == 0) nextItems.Remove(material.Key.itemCode);
            else nextItems[material.Key.itemCode] = new OwnedItem(material.Key, remaining);
        }
        var nextEquipment = new Dictionary<string, EquipmentInstance>(equipment, StringComparer.Ordinal);
        var created = new List<EquipmentInstance>();
        for (int i = 0; i < quantity; i++)
        {
            var instance = new EquipmentInstance(definition);
            nextEquipment.Add(instance.InstanceId, instance); created.Add(instance);
        }
        foreach (var instance in created) instance.Changed += OnInstanceChanged;
        items = nextItems; equipment = nextEquipment; Gold -= gold; MemoryFragments -= memory;
        Changed?.Invoke(); return true;
    }
    public void Clear()
    {
        foreach (var instance in equipment.Values) instance.Changed -= OnInstanceChanged;
        items.Clear(); equipment.Clear(); Gold = MemoryFragments = 0; Changed?.Invoke();
    }
}
