using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action Died;

    private void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead)
            return;

        currentHealth = Mathf.Max(
            currentHealth - damage,
            0
        );

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
