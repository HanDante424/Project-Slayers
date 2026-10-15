using System;
using UnityEngine;

public enum EffectHitPermission { Unspecified, Disabled, Enabled }

[Serializable]
public sealed class EffectAttackSettings
{
    public EffectHitPermission hitPermission = EffectHitPermission.Enabled;
    public SkillData skillData;
    public LearnedSkill skill = new();
    [Tooltip("SkillData hit entry. -1 means not yet assigned; never infer it from effect ordering.")]
    [Min(-1)] public int hitIndex = -1;
    [Min(0)] public float hitboxStartTime;
    [Tooltip("Seconds after spawn; -1 keeps detection active until effect disable/destruction.")]
    [Min(-1)] public float hitboxEndTime = -1f;

    public EffectAttackSettings() { }
    public EffectAttackSettings(EffectHitPermission permission) { hitPermission = permission; }
}
