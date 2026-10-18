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

    // Published once after HP is reduced, before death callbacks can despawn a pooled actor.
    public static event Action<Health, GameObject, int, bool> DamageApplied;
    public bool DamageEnabled { get; set; } = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetDamageEvents() => DamageApplied = null;

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

    public void TakeDamage(int damage) => TakeDamage(damage, null);

    public void TakeDamage(int damage, GameObject source)
    {
        if (!DamageEnabled || damage <= 0 || IsDead)
            return;

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(
            currentHealth - damage,
            0
        );

        DamageApplied?.Invoke(this, source, previousHealth - currentHealth, IsDead);
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
