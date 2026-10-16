using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayableCharacterData))]
[CanEditMultipleObjects]
public class PlayableCharacterDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Read-only feedback: classification never changes references or execution.
        foreach (Object selectedTarget in targets)
        {
            PlayableCharacterData data = (PlayableCharacterData)selectedTarget;
            DrawTierWarnings(data, data.passiveSkills, SkillTier.Passive, "Passive Skills");
            DrawTierWarnings(data, data.basicSkills, SkillTier.Basic, "Basic Skills");
            DrawTierWarnings(data, data.intermediateSkills, SkillTier.Intermediate, "Intermediate Skills");
            DrawTierWarnings(data, data.advancedSkills, SkillTier.Advanced, "Advanced Skills");
            DrawTierWarnings(data, data.ultimateSkills, SkillTier.Ultimate, "Ultimate Skills");
        }
    }

    private static void DrawTierWarnings(
        PlayableCharacterData data,
        List<SkillData> skills,
        SkillTier expectedTier,
        string listName)
    {
        if (skills == null)
        {
            return;
        }

        for (int i = 0; i < skills.Count; i++)
        {
            SkillData skill = skills[i];
            if (skill == null)
            {
                EditorGUILayout.HelpBox(
                    $"{data.name} / {listName} [{i}]: SkillData reference is missing.",
                    MessageType.Warning);
            }
            else if (skill.skillTier != expectedTier)
            {
                EditorGUILayout.HelpBox(
                    $"{data.name} / {listName} [{i}]: {skill.name} has Skill Tier " +
                    $"{skill.skillTier}; expected {expectedTier}. Update the tier or list manually. " +
                    "No reference has been moved or removed.",
                    MessageType.Warning);
            }
        }
    }
}
