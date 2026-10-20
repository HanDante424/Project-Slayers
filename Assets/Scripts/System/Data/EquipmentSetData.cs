using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentSetBonus
{
    [Min(1)] public int requiredEquipmentCount = 1;
    public List<EquipmentStatModifier> statModifiers = new();
    [Tooltip("Uses the same relative-percent unit as EquipmentData; applied when this set threshold is active.")]
    public float additionalStaggerGaugePercent;
}

[CreateAssetMenu(fileName = "EquipmentSetData", menuName = "Slayers/Equipment Set Data")]
public class EquipmentSetData : ScriptableObject
{
    public string setName;
    // Threshold definitions only. Runtime activation belongs to EquipmentStatEvaluator.
    public List<EquipmentSetBonus> bonuses = new();
}
