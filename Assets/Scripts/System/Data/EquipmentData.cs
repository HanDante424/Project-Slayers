using System.Collections.Generic;
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
    public List<EquipmentStatModifier> baseStatModifiers = new();
    public List<EquipmentStatModifier> detailStatModifiers = new();
    public List<EquipmentStatModifier> elementEnhancementModifiers = new();
    public List<EquipmentStatModifier> elementResistanceModifiers = new();

    [Header("Set")]
    public EquipmentSetData set;
}
