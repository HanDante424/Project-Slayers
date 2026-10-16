using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Slayers/Stage Database")]
public class StageDatabase : ScriptableObject
{
    [Header("Registered Stages")]
    [SerializeField] private List<MapData> stages = new();

    // Queries read current definitions and return copies; Inspector edits cannot leave a stale index.
    public MapData GetStageByCode(string stageCode)
    {
        if (string.IsNullOrWhiteSpace(stageCode) || stages == null)
            return null;

        foreach (MapData stage in stages)
        {
            if (stage != null && string.Equals(stage.stageCode, stageCode, StringComparison.Ordinal))
                return stage;
        }

        return null;
    }

    public List<MapData> GetAllStages() => GetSortedStages(null);

    public List<MapData> GetStagesByMapType(MapType mapType) =>
        GetSortedStages(stage => stage.mapType == mapType);

    public List<MapData> GetStagesByRank(int rank) =>
        GetSortedStages(stage => stage.rank == rank);

    public List<MapData> GetStagesByMapTypeAndRank(MapType mapType, int rank) =>
        GetSortedStages(stage => stage.mapType == mapType && stage.rank == rank);

    public List<MapData> GetDefaultUnlockedStages() =>
        GetSortedStages(stage => stage.unlockedByDefault);

    public List<int> GetRanksByMapType(MapType mapType)
    {
        var ranks = new HashSet<int>();
        if (stages != null)
        {
            foreach (MapData stage in stages)
            {
                if (stage != null && stage.mapType == mapType)
                    ranks.Add(stage.rank);
            }
        }

        var result = new List<int>(ranks);
        result.Sort();
        return result;
    }

    public List<string> GetValidationWarnings()
    {
        var warnings = new List<string>();
        if (stages == null)
        {
            warnings.Add("Stages 목록이 null입니다. Inspector에서 등록 목록을 확인하세요.");
            return warnings;
        }

        var registeredStages = new Dictionary<MapData, int>();
        var registeredCodes = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int index = 0; index < stages.Count; index++)
        {
            MapData stage = stages[index];
            if (stage == null)
            {
                warnings.Add($"Stages[{index}]: Null MapData가 등록되어 있습니다.");
                continue;
            }

            string location = $"Stages[{index}] ({stage.name})";
            if (registeredStages.TryGetValue(stage, out int previousStageIndex))
                warnings.Add($"{location}: 동일 SO가 Stages[{previousStageIndex}]에도 중복 등록되어 있습니다.");
            else
                registeredStages.Add(stage, index);

            string code = stage.stageCode;
            if (string.IsNullOrWhiteSpace(code))
            {
                warnings.Add($"{location}: Stage Code가 비어 있습니다.");
            }
            else
            {
                if (!IsNumericCode(code))
                    warnings.Add($"{location}: Stage Code '{code}'에 숫자(0-9)가 아닌 문자가 있습니다.");
                if (code.Length < 3)
                    warnings.Add($"{location}: Stage Code '{code}'는 3자리 미만입니다. 최소 3자리 숫자 문자열을 사용하세요.");
                if (registeredCodes.TryGetValue(code, out int previousCodeIndex))
                    warnings.Add($"{location}: Stage Code '{code}'가 Stages[{previousCodeIndex}]와 중복됩니다. 코드는 모든 MapType에서 고유해야 합니다.");
                else
                    registeredCodes.Add(code, index);
            }

            if (stage.unlockRequiredStage == stage)
                warnings.Add($"{location}: Unlock Required Stage가 자기 자신을 가리킵니다.");
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

    private List<MapData> GetSortedStages(Predicate<MapData> filter)
    {
        var result = new List<MapData>();
        if (stages != null)
        {
            foreach (MapData stage in stages)
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
