using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class SkillHitData
{
    [FormerlySerializedAs("damagePercent")]
    public float baseDamagePercent;
    public float damageGrowthPerLevel = 0f;
    public int damageCount = 1;
    public float hitStunDuration;
    public float knockbackPower;
    public float knockbackMultiplier = 1f;
    [FormerlySerializedAs("staggerMultiplier")]
    [Tooltip("Stagger amount as a percentage of the final damage dealt. 20 = 20%.")]
    public float staggerPercent = 0f;
    [InspectorName("Hit Effects")]
    public List<SkillHitEffectData> effects = new();
}

[Serializable]
public class SkillPrerequisite
{
    public SkillData skill;
    public int requiredLevel;
}

[CreateAssetMenu(
    fileName = "SkillData",
    menuName = "Slayers/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Identity")]
    public Sprite thumbnail;
    public string skillName;
    [TextArea(3, 8)] public string description;
    [Tooltip("Skill definition maximum level; configure per skill.")]
    public int maxLevel;

    [Header("Mechanism")]
    public SkillMechanism mechanism;

    [Header("Cost")]
    public SkillCostType costType;
    [Tooltip("Base cost at skill level 1; not connected to current resource consumption.")]
    public float costAmount;
    [Tooltip("Additional cost per skill level: costAmount + costGrowthPerLevel * (skillLevel - 1).")]
    public float costGrowthPerLevel;

    [Header("Prerequisites")]
    public List<SkillPrerequisite> prerequisites = new();

    [Header("Combat")]
    public float cooldown;
    public SkillAttackType attackType;
    public SkillElementType elementType;

    [Header("Classification")]
    [Tooltip("Progression tier only; Unassigned preserves skills awaiting classification. Does not control execution.")]
    public SkillTier skillTier;
    public SkillCategory category;
    public List<SkillType> skillTypes = new();

    [Header("Classification Effects")]
    public List<SkillClassificationEffectData> effects = new();

    [Header("Hits")]
    public SkillHitData[] hits;

    public int GetEffectiveLevel(int level)
    {
        return Mathf.Clamp(level, 1, Mathf.Max(1, maxLevel));
    }

    public float GetCostAtLevel(int level)
    {
        int effectiveLevel = GetEffectiveLevel(level);
        return costAmount + costGrowthPerLevel * (effectiveLevel - 1);
    }

    public bool TryGetHitData(
        int hitIndex,
        out SkillHitData hitData)
    {
        hitData = null;

        if (hits == null ||
            hitIndex < 0 ||
            hitIndex >= hits.Length)
        {
            return false;
        }

        hitData = hits[hitIndex];
        return hitData != null;
    }
}
