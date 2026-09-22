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
    public MapType mapType;
    public MapRank mapRank;

    [Header("Rewards")]
    [Min(0)] public int goldReward;
    [Min(0)] public int memoryFragmentReward;

    [Header("Enemies")]
    public List<MonsterData> enemies = new();

    [Header("Quests")]
    public List<QuestData> quests = new();
}
