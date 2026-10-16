using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Slayers/Stage Database")]
public class StageDatabase : ScriptableObject
{
    [Header("Search Stages")]
    [FormerlySerializedAs("stages")]
    [SerializeField] private List<MapData> searchStages = new();
    [Header("Defense Stages")]
    [SerializeField] private List<MapData> defenseStages = new();
    [Header("Attack Stages")]
    [SerializeField] private List<MapData> attackStages = new();
    [Header("Conquest Stages")]
    [SerializeField] private List<MapData> conquestStages = new();
    [Header("Raid Stages")]
    [SerializeField] private List<MapData> raidStages = new();

    private static readonly MapType[] Categories =
        { MapType.Search, MapType.Defense, MapType.Attack, MapType.Conquest, MapType.Raid };

    private List<MapData> GetCategory(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.Search: return searchStages;
            case MapType.Defense: return defenseStages;
            case MapType.Attack: return attackStages;
            case MapType.Conquest: return conquestStages;
            case MapType.Raid: return raidStages;
            default: return null;
        }
    }

    private IEnumerable<MapData> EnumerateAllStages()
    {
        foreach (MapType category in Categories)
        {
            List<MapData> entries = GetCategory(category);
            if (entries == null) continue;
            foreach (MapData stage in entries) yield return stage;
        }
    }

    // Queries read current definitions and return copies; Inspector edits cannot leave a stale index.
    public MapData GetStageByCode(string stageCode)
    {
        if (string.IsNullOrWhiteSpace(stageCode))
            return null;

        foreach (MapData stage in EnumerateAllStages())
        {
            if (stage != null && string.Equals(stage.stageCode, stageCode, StringComparison.Ordinal))
                return stage;
        }

        return null;
    }

    public List<MapData> GetAllStages() => GetSortedStages(EnumerateAllStages(), null);

    public List<MapData> GetStagesByMapType(MapType mapType) =>
        GetSortedStages(GetCategory(mapType), null);

    public List<MapData> GetStagesByRank(int rank) =>
        GetSortedStages(EnumerateAllStages(), stage => stage.rank == rank);

    public List<MapData> GetStagesByMapTypeAndRank(MapType mapType, int rank) =>
        GetSortedStages(GetCategory(mapType), stage => stage.rank == rank);

    public List<MapData> GetDefaultUnlockedStages() =>
        GetSortedStages(EnumerateAllStages(), stage => stage.unlockedByDefault);

    public List<int> GetRanksByMapType(MapType mapType)
    {
        var ranks = new HashSet<int>();
        List<MapData> entries = GetCategory(mapType);
        if (entries != null)
        {
            foreach (MapData stage in entries)
            {
                if (stage != null) ranks.Add(stage.rank);
            }
        }

        var result = new List<int>(ranks);
        result.Sort();
        return result;
    }

    public List<string> GetValidationWarnings()
    {
        var warnings = new List<string>();
        var registeredStages = new Dictionary<MapData, string>();
        var registeredCodes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (MapType category in Categories)
        {
            List<MapData> entries = GetCategory(category);
            if (entries == null)
            {
                warnings.Add($"{category} Stages 목록이 null입니다.");
                continue;
            }
            for (int index = 0; index < entries.Count; index++)
            {
                MapData stage = entries[index];
                string location = $"{category} Stages[{index}]";
                if (stage == null)
                {
                    warnings.Add($"{location}: Null MapData 또는 유실된 스테이지 참조가 등록되어 있습니다.");
                    continue;
                }
                location += $" ({stage.name})";
                if (stage.mapType != category)
                    warnings.Add($"{location}: MapType {stage.mapType}과 등록 카테고리 {category}가 불일치합니다.");
                if (registeredStages.TryGetValue(stage, out string previousStage))
                    warnings.Add($"{location}: 동일 MapData가 {previousStage}에도 중복 등록되어 있습니다.");
                else
                    registeredStages.Add(stage, location);

                string code = stage.stageCode;
                if (string.IsNullOrWhiteSpace(code))
                    warnings.Add($"{location}: Stage Code가 비어 있습니다.");
                else
                {
                    if (!IsNumericCode(code))
                        warnings.Add($"{location}: Stage Code '{code}'에 숫자(0-9)가 아닌 문자가 있습니다.");
                    if (code.Length < 3)
                        warnings.Add($"{location}: Stage Code '{code}'는 3자리 미만입니다.");
                    if (registeredCodes.TryGetValue(code, out string previousCode))
                        warnings.Add($"{location}: Stage Code '{code}'가 {previousCode}와 중복됩니다. 코드는 전체 카테고리에서 고유해야 합니다.");
                    else
                        registeredCodes.Add(code, location);
                }
                if (stage.unlockRequiredStage == stage)
                    warnings.Add($"{location}: Unlock Required Stage가 자기 자신을 가리킵니다.");
            }
        }
        return warnings;
    }

    [ContextMenu("Validate Stages")]
    public void ValidateStages()
    {
        foreach (string warning in GetValidationWarnings())
            Debug.LogWarning($"[StageDatabase] {warning}", this);
    }

    private void OnValidate()
    {
        ValidateStages();
    }

    private List<MapData> GetSortedStages(IEnumerable<MapData> entries, Predicate<MapData> filter)
    {
        var result = new List<MapData>();
        if (entries != null)
        {
            foreach (MapData stage in entries)
            {
                if (stage != null && (filter == null || filter(stage)))
                    result.Add(stage);
            }
        }

        result.Sort((left, right) => CompareStageCodes(left.stageCode, right.stageCode));
        return result;
    }

    private static bool IsNumericCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        foreach (char digit in code)
        {
            if (digit < '0' || digit > '9')
                return false;
        }

        return true;
    }

    private static int CompareStageCodes(string left, string right)
    {
        bool leftNumeric = IsNumericCode(left);
        bool rightNumeric = IsNumericCode(right);
        if (leftNumeric != rightNumeric)
            return leftNumeric ? -1 : 1;
        if (!leftNumeric)
            return string.CompareOrdinal(left, right);

        // Compare numeric magnitude without integer conversion, preserving arbitrarily long codes.
        int leftStart = 0;
        int rightStart = 0;
        while (leftStart < left.Length - 1 && left[leftStart] == '0')
            leftStart++;
        while (rightStart < right.Length - 1 && right[rightStart] == '0')
            rightStart++;

        int leftLength = left.Length - leftStart;
        int rightLength = right.Length - rightStart;
        int comparison = leftLength.CompareTo(rightLength);
        if (comparison == 0)
            comparison = string.CompareOrdinal(left, leftStart, right, rightStart, leftLength);

        // Codes remain exact string identifiers even if their numeric values are equal.
        return comparison != 0 ? comparison : string.CompareOrdinal(left, right);
    }
}
