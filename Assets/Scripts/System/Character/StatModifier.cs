using System;
using UnityEngine;

[Serializable]
public sealed class StatModifier
{
    [SerializeField] private StatType statType;
    [SerializeField] private StatModifierType modifierType;
    [SerializeField] private float value;
    [SerializeField] private string sourceId;

    public StatType StatType => statType;
    public StatModifierType ModifierType => modifierType;
    public float Value => value;
    public string SourceId => sourceId;

    public StatModifier(
        StatType statType,
        StatModifierType modifierType,
        float value,
        string sourceId)
    {
        this.statType = statType;
        this.modifierType = modifierType;
        this.value = value;
        this.sourceId = sourceId;
    }
}
