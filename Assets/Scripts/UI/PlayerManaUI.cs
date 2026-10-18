using UnityEngine;
using UnityEngine.UI;

public class PlayerManaUI : MonoBehaviour
{
    [SerializeField] private PlayerMana playerMana;
    [SerializeField] private Image manaCover;

    public void Bind(PlayerMana target)
    {
        if (isActiveAndEnabled) OnDisable();
        playerMana = target;
        if (isActiveAndEnabled) OnEnable();
    }

    private void OnEnable()
    {
        if (playerMana == null)
            return;

        // Entry binding may run before Unity invokes this view's first OnEnable.
        playerMana.ManaChanged -= UpdateMana;
        playerMana.ManaChanged += UpdateMana;
        UpdateMana(playerMana.CurrentMana, playerMana.MaxMana);
    }

    private void OnDisable()
    {
        if (playerMana == null)
            return;

        playerMana.ManaChanged -= UpdateMana;
    }

    private void UpdateMana(float currentMana, float maxMana)
    {
        if (manaCover == null)
            return;

        float manaRatio = maxMana > 0f
            ? Mathf.Clamp01(currentMana / maxMana)
            : 0f;

        manaCover.fillAmount = 1f - manaRatio;
    }
}
