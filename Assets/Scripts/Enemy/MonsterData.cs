using System.Collections.Generic;
using UnityEngine;

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
}
