using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentSetBonus
{
    [Min(1)] public int requiredEquipmentCount = 1;
    public List<EquipmentStatModifier> statModifiers = new();
    [Tooltip("Definition only; uses the same relative-percent unit as EquipmentData.")]
    public float additionalStaggerGaugePercent;
}

[CreateAssetMenu(fileName = "EquipmentSetData", menuName = "Slayers/Equipment Set Data")]
public class EquipmentSetData : ScriptableObject
{
    public string setName;
    // Threshold definitions only. Inventory/set activation remains owned by a future evaluator.
    public List<EquipmentSetBonus> bonuses = new();
}
