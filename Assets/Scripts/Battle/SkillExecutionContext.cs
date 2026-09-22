using UnityEngine;

public class SkillExecutionContext
{
    public GameObject Owner { get; }
    public CharacterStats CharacterStats { get; }
    public Animator Animator { get; }
    public Transform EffectSpawnPoint { get; }
    public int SkillLevel { get; }

    public SkillExecutionContext(
        GameObject owner,
        CharacterStats characterStats,
        Animator animator,
        Transform effectSpawnPoint,
        int skillLevel)
    {
        Owner = owner;
        CharacterStats = characterStats;
        Animator = animator;
        EffectSpawnPoint = effectSpawnPoint;
        SkillLevel = Mathf.Max(1, skillLevel);
    }
}
