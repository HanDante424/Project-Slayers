using System;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private StageDatabase stageDatabase;

    // Override this boundary when player unlock records become available.
    public virtual bool IsStageUnlocked(MapData stage) => stage != null && stage.unlockedByDefault;

    public List<MapData> GetAvailableStages(MapType mapType)
    {
        var result = new List<MapData>();
        if (stageDatabase == null) return result;

        var seenCodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (MapData stage in stageDatabase.GetStagesByMapType(mapType))
        {
            if (stage == null || stage.mapType != mapType || !IsStageUnlocked(stage) ||
                !IsValidCode(stage.stageCode) || !seenCodes.Add(stage.stageCode))
                continue;
            result.Add(stage);
        }
        return result;
    }

    public List<MapData> GetAvailableStages(MapType mapType, int rank) =>
        GetAvailableStages(mapType).FindAll(stage => stage.rank == rank);

    public List<int> GetAvailableRanks(MapType mapType)
    {
        var ranks = new SortedSet<int>();
        foreach (MapData stage in GetAvailableStages(mapType)) ranks.Add(stage.rank);
        return new List<int>(ranks);
    }

    private static bool IsValidCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length < 3) return false;
        foreach (char digit in code)
            if (digit < '0' || digit > '9') return false;
        return true;
    }
}
