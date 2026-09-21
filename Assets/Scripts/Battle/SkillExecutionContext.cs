using UnityEngine;

public class SkillExecutionContext
{
    public GameObject Owner { get; }
    public CharacterStats CharacterStats { get; }
    public Animator Animator { get; }
    public Transform EffectSpawnPoint { get; }

    public SkillExecutionContext(
        GameObject owner,
        CharacterStats characterStats,
        Animator animator,
        Transform effectSpawnPoint)
    {
        Owner = owner;
        CharacterStats = characterStats;
        Animator = animator;
        EffectSpawnPoint = effectSpawnPoint;
    }
}
