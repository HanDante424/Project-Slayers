using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillHitData
{
    public int damagePercent;
    public float hitStunDuration;
    public float knockbackPower;
    public float knockbackMultiplier = 1f;
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
    public string skillName;
    public float cooldown;
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
