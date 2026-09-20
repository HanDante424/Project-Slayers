using System.Collections.Generic;
using UnityEngine;

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

    [Header("Skills")]
    public List<SkillData> skills = new();
}
