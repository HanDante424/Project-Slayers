using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        currentHealth = Mathf.Max(
            currentHealth - damage,
            0
        );

        Debug.Log(
            $"{name} HP: {currentHealth} / {maxHealth}"
        );
    }
}