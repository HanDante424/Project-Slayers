using System;

public enum EquipmentSlot
{
    Unassigned = -1,
    // Legacy values 0..5 (Weapon..Accessory) are reserved, never reinterpreted.
    Belt = 10,
    Bracelet = 11,
    Necklace = 12,
    Gloves = 13,
    HairAccessory = 14,
    BackAccessory = 15,
    Earrings = 16,
    Rings = 17,
    Badge = 18
}

public static class EquipmentSlotUtility
{
    public static bool IsValid(EquipmentSlot slot) => slot >= EquipmentSlot.Belt && slot <= EquipmentSlot.Badge;
    public static bool TryParse(string key, out EquipmentSlot slot)
        => Enum.TryParse(key, out slot) && IsValid(slot) && slot.ToString() == key;
}
