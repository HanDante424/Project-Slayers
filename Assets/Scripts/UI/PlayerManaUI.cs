using UnityEngine;
using UnityEngine.UI;

public class PlayerManaUI : MonoBehaviour
{
    [SerializeField] private PlayerMana playerMana;
    [SerializeField] private Image manaFill;

    private void OnEnable()
    {
        if (playerMana == null)
            return;

        playerMana.ManaChanged += UpdateManaBar;
        UpdateManaBar(playerMana.CurrentMana, playerMana.MaxMana);
    }

    private void OnDisable()
    {
        if (playerMana == null)
            return;

        playerMana.ManaChanged -= UpdateManaBar;
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaFill == null)
            return;

        manaFill.fillAmount = maxMana > 0f
            ? currentMana / maxMana
            : 0f;
    }
}
