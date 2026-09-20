using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image healthCover;

    private void OnEnable()
    {
        if (health == null)
            return;

        health.HealthChanged += UpdateHealth;
        UpdateHealth(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        if (health == null)
            return;

        health.HealthChanged -= UpdateHealth;
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthCover == null)
            return;

        float healthRatio = maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / maxHealth)
            : 0f;

        healthCover.fillAmount = 1f - healthRatio;
    }
}
