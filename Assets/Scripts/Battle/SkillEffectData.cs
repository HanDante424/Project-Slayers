using System;

// Coefficient meaning (or whether it is used) belongs to the mechanism.
[Serializable]
public class SkillEffectData
{
    public SkillEffectType effectType;
    public float baseCoefficient;
    public float coefficientGrowthPerLevel;
}
