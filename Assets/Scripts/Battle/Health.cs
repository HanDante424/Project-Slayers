using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action Died;
    public event Action<int, int> HealthChanged;

    private void Awake()
    {
        ResetHealth();
    }

    public void SetMaxHealth(int value, bool resetCurrent = true)
    {
        maxHealth = Mathf.Max(1, value);
        currentHealth = resetCurrent
            ? maxHealth
            : Mathf.Clamp(currentHealth, 0, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
            return;

        currentHealth = Mathf.Max(
            currentHealth - damage,
            0
        );

        HealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log(
            $"{name} HP: {currentHealth} / {maxHealth}"
        );

        if (IsDead)
        {
            Debug.Log($"{name} Dead");
            Died?.Invoke();
        }
    }
}
