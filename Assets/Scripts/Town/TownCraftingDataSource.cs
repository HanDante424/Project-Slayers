using System;
using UnityEngine;

// Adapter for a future authoritative inventory/wallet owner. No progress is stored in SO definitions.
public abstract class TownCraftingDataSource : MonoBehaviour
{
    public event Action Changed;
    protected void NotifyChanged() => Changed?.Invoke();
    public abstract long GetOwnedMaterialCount(ItemData material);
    public abstract bool CanCraft(EquipmentData equipment, int quantity);
    // Implementations must atomically validate costs, consume inventory and grant equipment instances.
    public abstract bool TryCraft(EquipmentData equipment, int quantity);
}
