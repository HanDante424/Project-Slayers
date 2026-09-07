using UnityEngine;

public class TownMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject researchPanel;
    [SerializeField] private GameObject storagePanel;
    [SerializeField] private GameObject optionPanel;

    private void Start()
    {
        OpenBattle();
    }

    public void OpenBattle()
    {
        ShowPanel(battlePanel);
    }

    public void OpenCharacter()
    {
        ShowPanel(characterPanel);
    }

    public void OpenShop()
    {
        ShowPanel(shopPanel);
    }

    public void OpenResearch()
    {
        ShowPanel(researchPanel);
    }

    public void OpenStorage()
    {
        ShowPanel(storagePanel);
    }

    public void OpenOption()
    {
        ShowPanel(optionPanel);
    }

    private void ShowPanel(GameObject targetPanel)
    {
        if (battlePanel != null)
            battlePanel.SetActive(battlePanel == targetPanel);

        if (characterPanel != null)
            characterPanel.SetActive(characterPanel == targetPanel);

        if (shopPanel != null)
            shopPanel.SetActive(shopPanel == targetPanel);

        if (researchPanel != null)
            researchPanel.SetActive(researchPanel == targetPanel);

        if (storagePanel != null)
            storagePanel.SetActive(storagePanel == targetPanel);

        if (optionPanel != null)
            optionPanel.SetActive(optionPanel == targetPanel);
    }
}