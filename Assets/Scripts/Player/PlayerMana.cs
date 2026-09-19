using System;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class PlayerMana : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;

    private float currentMana;

    public float CurrentMana => currentMana;
    public float MaxMana
    {
        get
        {
            // UI OnEnable can run before this component's Awake.
            if (characterStats == null)
                characterStats = GetComponent<CharacterStats>();

            return characterStats.FinalMaxMana;
        }
    }

    public event Action<float, float> ManaChanged;

    private void Awake()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        ResetMana();
    }

    public void ResetMana()
    {
        currentMana = MaxMana;
        ManaChanged?.Invoke(currentMana, MaxMana);
    }

    public bool CanSpend(float amount)
    {
        return !float.IsNaN(amount) &&
            !float.IsInfinity(amount) &&
            amount >= 0f && currentMana >= amount;
    }

    public bool Spend(float amount)
    {
        if (!CanSpend(amount))
            return false;

        currentMana -= amount;
        ManaChanged?.Invoke(currentMana, MaxMana);
        return true;
    }
}
