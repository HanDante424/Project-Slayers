using System;

// Skill-wide classification only; separate from effects applied by a hit.
[Serializable]
public class SkillClassificationEffectData
{
    public EffectTargetType target;
    public EffectCategoryType category;
    public SkillClassificationDamageEffectType damageType;
    public SkillClassificationBuffEffectType buffType;
    public SkillClassificationDebuffEffectType debuffType;
    public SkillClassificationUniqueEffectType uniqueType;
}
