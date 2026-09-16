using System;
using UnityEngine;

[Serializable]
public class SkillHitData
{
    public int damagePercent;
    public float hitStunDuration;
    public float knockbackPower;
}

[CreateAssetMenu(
    fileName = "SkillData",
    menuName = "Slayers/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public SkillHitData[] hits;

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
