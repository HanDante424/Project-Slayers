using UnityEngine;

public sealed class InventoryCraftingDataSource : TownCraftingDataSource
{
    private SharedInventory inventory;
    private void OnEnable() { inventory = InventorySession.Inventory; inventory.Changed += NotifyChanged; }
    private void OnDisable() { if (inventory != null) inventory.Changed -= NotifyChanged; }
    public override long GetOwnedMaterialCount(ItemData material) => InventorySession.Inventory.GetCount(material);
    public override bool CanCraft(EquipmentData equipment, int quantity) => InventorySession.Inventory.CanCraft(equipment, quantity);
    public override bool TryCraft(EquipmentData equipment, int quantity) => InventorySession.Inventory.TryCraft(equipment, quantity);
}
