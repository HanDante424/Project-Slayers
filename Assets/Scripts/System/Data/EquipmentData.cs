using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Slayers/Equipment Data")]
public class EquipmentData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Spreadsheet numeric code. -1 means not assigned; 0 is a valid code.")]
    public int equipmentCode = -1;
    public string equipmentEnglishName;
    public Sprite thumbnail;
    public string equipmentName;
    [TextArea(3, 8)] public string description;

    [Header("Classification")]
    public int rank = 1;
    public EquipmentSlot slot = EquipmentSlot.Unassigned;

    [Header("Stats")]
    public List<EquipmentStatModifier> baseStatModifiers = new();
    public List<EquipmentStatModifier> detailStatModifiers = new();
    public List<EquipmentStatModifier> elementEnhancementModifiers = new();
    public List<EquipmentStatModifier> elementResistanceModifiers = new();
    [Tooltip("Definition only; not yet applied at runtime. 30 means +30% of the base stagger-gauge health ratio, not +30 percentage points.")]
    public float additionalStaggerGaugePercent;

    [Header("Crafting")]
    public int craftingGold;
    public int craftingMemoryFragments;
    public List<EquipmentCraftingMaterial> craftingMaterials = new();

    [Header("Set")]
    public EquipmentSetData set;
}
