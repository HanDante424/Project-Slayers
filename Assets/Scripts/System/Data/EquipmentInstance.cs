using System;
using System.Collections.Generic;
using UnityEngine;

// Owned runtime state. Definition assets are never modified by instance upgrades.
[Serializable]
public sealed class EquipmentInstance
{
    [SerializeField] private string instanceId;
    [SerializeField] private EquipmentData definition;
    [SerializeField] private int enhancementLevel;
    [SerializeField] private List<StatModifier> additionalModifiers = new();
    [SerializeField] private List<EquipmentSetData> additionalSets = new();
    public string InstanceId => instanceId;
    public EquipmentData Definition => definition;
    public int EnhancementLevel => enhancementLevel;
    public IReadOnlyList<StatModifier> AdditionalModifiers => additionalModifiers.AsReadOnly();
    public IReadOnlyList<EquipmentSetData> AdditionalSets => additionalSets.AsReadOnly();
    public event Action Changed;

    public EquipmentInstance(EquipmentData definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        this.definition = definition;
        instanceId = Guid.NewGuid().ToString("N");
    }

    public bool SetEnhancementLevel(int value)
    {
        if (value < 0) return false;
        if (enhancementLevel == value) return true;
        enhancementLevel = value;
        Changed?.Invoke();
        return true;
    }

    public bool AddModifier(StatType stat, StatModifierType type, float value)
    {
        if (!Enum.IsDefined(typeof(StatType), stat) || !Enum.IsDefined(typeof(StatModifierType), type) ||
            float.IsNaN(value) || float.IsInfinity(value)) return false;
        additionalModifiers.Add(new StatModifier(stat, type, value, instanceId));
        Changed?.Invoke();
        return true;
    }

    public bool AddSet(EquipmentSetData set)
    {
        if (set == null || set == definition.set || additionalSets.Contains(set)) return false;
        additionalSets.Add(set);
        Changed?.Invoke();
        return true;
    }
}
