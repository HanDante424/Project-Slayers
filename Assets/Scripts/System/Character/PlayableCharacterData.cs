using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartingSkillEntry
{
    public SkillData skill;

    [Min(1)]
    public int startingLevel = 1;
}

[CreateAssetMenu(
    fileName = "PlayableCharacterData",
    menuName = "Slayers/Character/Playable Character Data")]
public class PlayableCharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;

    [Header("Images")]
    public Sprite thumbnail;
    public Sprite fullBodyImage;

    [Header("Descriptions")]
    public List<DescriptionEntry> descriptions = new();

    [Header("Stats")]
    public CharacterStatsData statsData;

    [Header("Starting Skills")]
    public List<StartingSkillEntry> startingSkills = new();

    [Header("Passive Skills")]
    public List<SkillData> passiveSkills = new();

    [Header("Basic Skills")]
    public List<SkillData> basicSkills = new();

    [Header("Intermediate Skills")]
    public List<SkillData> intermediateSkills = new();

    [Header("Advanced Skills")]
    public List<SkillData> advancedSkills = new();

    [Header("Ultimate Skills")]
    public List<SkillData> ultimateSkills = new();
}
