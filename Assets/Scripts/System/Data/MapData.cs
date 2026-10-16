using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MapData",
    menuName = "Slayers/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Stage Identity")]
    public string stageCode;

    [Header("Identity")]
    public Sprite thumbnail;
    public string mapName;
    [TextArea(3, 8)] public string description;

    [Header("Classification")]
    [Min(1)] public int rank = 1;
    [Min(0)] public int corruption;
    public MapType mapType;

    [Header("Unlock")]
    // Definition data only; player unlock progress belongs in separate save data.
    public bool unlockedByDefault;
    // Future validation should reject self references and prerequisite cycles.
    public MapData unlockRequiredStage;

    [Header("Enemies")]
    public List<MonsterData> enemies = new();

    [Header("Rewards")]
    [Min(0)] public int goldReward;
    [Min(0)] public int memoryFragmentReward;
}
