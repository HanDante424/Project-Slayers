using System;
using UnityEngine;

// Per-hit effect definition; interpretation belongs to a future mechanism.
[Serializable]
public class SkillHitEffectData
{
    public EffectTargetType target;
    public EffectCategoryType category;
    public SkillHitDamageEffectType damageType;
    public SkillHitBuffEffectType buffType;
    public SkillHitDebuffEffectType debuffType;
    public SkillHitUniqueEffectType uniqueType;

    [Tooltip("Base option value; interpretation belongs to the future effect mechanism.")]
    public float optionValue;
    [Tooltip("Additional option value per skill level after level 1.")]
    public float optionGrowthPerLevel;
}
