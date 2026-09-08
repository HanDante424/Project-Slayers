using UnityEngine;

public class CharacterStatsModifierTest : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;


    [ContextMenu("Test/Add Attack Modifiers")]
    private void AddAttackModifiers()
    {
        characterStats.AddModifier(
            new StatModifier(
                StatType.Attack,
                StatModifierType.Flat,
                20f,
                "test_attack"
            )
        );

        characterStats.AddModifier(
            new StatModifier(
                StatType.Attack,
                StatModifierType.Percent,
                10f,
                "test_attack"
            )
        );

        Debug.Log(
            $"[Stat Test] Attack Current: {characterStats.Attack:F2} / " +
            $"Final: {characterStats.FinalAttack:F2}"
        );
    }


    [ContextMenu("Test/Remove Attack Modifiers")]
    private void RemoveAttackModifiers()
    {
        int removed =
            characterStats.RemoveModifiersBySource("test_attack");

        Debug.Log(
            $"[Stat Test] Removed: {removed} / " +
            $"Final Attack: {characterStats.FinalAttack:F2}"
        );
    }


    [ContextMenu("Test/Add Health Modifier")]
    private void AddHealthModifier()
    {
        characterStats.AddModifier(
            new StatModifier(
                StatType.Health,
                StatModifierType.Flat,
                100f,
                "test_health"
            )
        );

        Debug.Log(
            $"[Stat Test] Health Current: {characterStats.MaxHealth:F2} / " +
            $"Final: {characterStats.FinalMaxHealth:F2} / " +
            $"Stagger Gauge: {characterStats.FinalStaggerGauge:F2}"
        );
    }


    [ContextMenu("Test/Clear All Modifiers")]
    private void ClearAllModifiers()
    {
        characterStats.ClearModifiers();

        Debug.Log(
            $"[Stat Test] Modifiers Cleared / Count: {characterStats.ModifierCount}"
        );
    }


    [ContextMenu("Test/Set Level 1")]
    private void SetLevel1()
    {
        characterStats.SetLevel(1);

        Debug.Log(
            $"[Stat Test] Level 1 / Attack: {characterStats.FinalAttack:F2}"
        );
    }


    [ContextMenu("Test/Set Level 50")]
    private void SetLevel50()
    {
        characterStats.SetLevel(50);

        Debug.Log(
            $"[Stat Test] Level 50 / Attack: {characterStats.FinalAttack:F2}"
        );
    }
}