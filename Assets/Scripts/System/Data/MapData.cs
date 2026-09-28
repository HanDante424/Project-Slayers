using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MapData",
    menuName = "Slayers/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Identity")]
    public Sprite thumbnail;
    public string mapName;
    [TextArea(3, 8)] public string description;

    [Header("Classification")]
    [Min(1)] public int rank = 1;
    [Min(0)] public int corruption;
    public MapType mapType;

    [Header("Enemies")]
    public List<MonsterData> enemies = new();

    [Header("Rewards")]
    [Min(0)] public int goldReward;
    [Min(0)] public int memoryFragmentReward;
}
