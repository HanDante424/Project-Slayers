using System;
using System.Collections.Generic;
using UnityEngine;

// Definition references only. Owned quantities, equipped instances and crafting state live elsewhere.
[DisallowMultipleComponent]
public sealed class ItemEquipmentRegistry : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private List<ItemData> items = new();
    [Header("Equipment")]
    [SerializeField] private List<EquipmentData> equipment = new();

    // Item and equipment codes have independent namespaces (both may contain code 0).
    // Invalid registrations are reported in the Inspector; a duplicate code resolves to the first entry.
    public ItemData GetItemByCode(int itemCode)
    {
        if (items != null)
            foreach (var item in items)
                if (item != null && item.itemCode == itemCode) return item;
        return null;
    }

    public EquipmentData GetEquipmentByCode(int equipmentCode)
    {
        if (equipment != null)
            foreach (var definition in equipment)
                if (definition != null && definition.equipmentCode == equipmentCode) return definition;
        return null;
    }

    // Fresh sorted lists keep callers from changing Inspector registration order.
    public List<ItemData> GetAllItems() => GetSortedDefinitions(items, item => item.itemCode);
    public List<EquipmentData> GetAllEquipment() => GetSortedDefinitions(equipment, definition => definition.equipmentCode);

    public IEnumerable<string> GetValidationWarnings()
    {
        foreach (var warning in ValidateDefinitions(items, item => item.itemCode, "Items")) yield return warning;
        foreach (var warning in ValidateDefinitions(equipment, definition => definition.equipmentCode, "Equipment")) yield return warning;
    }

    private static List<T> GetSortedDefinitions<T>(List<T> source, Func<T, int> code) where T : UnityEngine.Object
    {
        var result = new List<T>();
        var seen = new HashSet<T>();
        if (source != null)
            foreach (var definition in source)
                if (definition != null && seen.Add(definition)) result.Add(definition);
        result.Sort((left, right) =>
        {
            int comparison = code(left).CompareTo(code(right));
            return comparison != 0 ? comparison : string.Compare(left.name, right.name, StringComparison.Ordinal);
        });
        return result;
    }

    private static IEnumerable<string> ValidateDefinitions<T>(List<T> source, Func<T, int> code, string category) where T : UnityEngine.Object
    {
        if (source == null) yield break;
        var definitions = new HashSet<T>();
        var codes = new HashSet<int>();
        for (int index = 0; index < source.Count; index++)
        {
            var definition = source[index];
            if (definition == null)
            {
                yield return $"{category}[{index}]: SO 참조가 비어 있습니다.";
                continue;
            }
            if (!definitions.Add(definition))
                yield return $"{category}[{index}]: 같은 SO가 중복 등록되었습니다 ({definition.name}).";
            if (!codes.Add(code(definition)))
                yield return $"{category}[{index}]: 코드 {code(definition)}가 중복되었습니다. 코드 조회는 첫 등록 항목을 반환합니다.";
        }
    }
}
