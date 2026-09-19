using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image damageCover;

    private void OnEnable()
    {
        if (health == null)
            return;

        health.HealthChanged += UpdateHealthBar;
        UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.HealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (damageCover == null || maxHealth <= 0)
            return;

        float healthRatio = (float)currentHealth / maxHealth;
        damageCover.fillAmount = 1f - healthRatio;
    }
    public void Bind(Health targetHealth)
    {
        if (health != null)
        {
            health.HealthChanged -= UpdateHealthBar;
        }

        health = targetHealth;

        if (health == null)
            return;

        health.HealthChanged += UpdateHealthBar;
        UpdateHealthBar(
            health.CurrentHealth,
            health.MaxHealth
        );
    }
}