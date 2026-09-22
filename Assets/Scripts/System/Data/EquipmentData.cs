using UnityEngine;

[CreateAssetMenu(
    fileName = "EquipmentData",
    menuName = "Slayers/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("Identity")]
    public Sprite thumbnail;
    public string equipmentName;
    [TextArea(3, 8)] public string description;

    [Header("Classification")]
    public EquipmentRank rank;
    public EquipmentSlot slot;

    [Header("Stats")]
    public EquipmentBaseStats baseStats = new();
    public EquipmentDetailStats detailStats = new();
    public EquipmentElementEnhancementStats elementEnhancementStats = new();
    public EquipmentElementResistanceStats elementResistanceStats = new();

    [Header("Set")]
    public EquipmentSetData set;
}
