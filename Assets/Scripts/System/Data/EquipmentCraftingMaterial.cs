using System;

// Definition data only; inventory checks and consumption belong to the future crafting system.
[Serializable]
public class EquipmentCraftingMaterial
{
    public ItemData material;
    public int quantity;
}
