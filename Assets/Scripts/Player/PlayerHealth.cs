using UnityEngine;

[RequireComponent(typeof(CharacterStats), typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private Health health;

    private void Awake()
    {
        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();
        if (health == null)
            health = GetComponent<Health>();
    }

    private void Start()
    {
        // Apply after Health and CharacterStats have completed Awake.
        health.SetMaxHealth(Mathf.RoundToInt(characterStats.FinalMaxHealth));
    }
}
