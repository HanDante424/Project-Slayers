using System;
using UnityEngine;

[Serializable]
public class LearnedSkill
{
    // The existing controller field owns the serialized definition reference.
    [NonSerialized] private SkillData skillData;
    [SerializeField, Min(1)] private int currentLevel = 1;

    public SkillData SkillData => skillData;
    public int CurrentLevel => skillData != null
        ? skillData.GetEffectiveLevel(currentLevel)
        : 1;

    public void Bind(SkillData definition)
    {
        skillData = definition;
    }
}
