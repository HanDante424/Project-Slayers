using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterItemDrop
{
    public ItemData item;
    public float dropChance;
}

[CreateAssetMenu(
    fileName = "MonsterData",
    menuName = "Slayers/Monster/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Identity")]
    public string monsterName;

    [Header("Images")]
    public Sprite thumbnail;
    public Sprite fullBodyImage;

    [Header("Descriptions")]
    public List<DescriptionEntry> descriptions = new();

    [Header("Stats")]
    public CharacterStatsData statsData;

    [Header("Skills")]
    public List<SkillData> skills = new();

    [Header("Item Drops")]
    public List<MonsterItemDrop> itemDrops = new();
}
