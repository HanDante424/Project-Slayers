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
    [Tooltip("Fixed multiplier of damage dealt. Not applied by the current runtime.")]
    public float staggerMultiplier = 0f;
    public List<SkillEffectData> effects = new();
}

[Serializable]
public class SkillPrerequisite
{
    public SkillData skill;
    public int requiredLevel;
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
    [Tooltip("Interpretation is defined by the future mechanism or runtime.")]
    public float costAmount;

    [Header("Prerequisites")]
    public List<SkillPrerequisite> prerequisites = new();

    [Header("Combat")]
    public float cooldown;
    public SkillAttackType attackType;
    public SkillElementType elementType;

    [Header("Classification")]
    public SkillCategory category;
    public List<SkillType> skillTypes = new();

    [Header("Effects")]
    public List<SkillEffectData> effects = new();

    [Header("Hits")]
    public SkillHitData[] hits;
    public SkillMechanism mechanism;
    public List<SkillPrerequisite> prerequisites = new();

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
