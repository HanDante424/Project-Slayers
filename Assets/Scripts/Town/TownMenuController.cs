using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TownMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject researchPanel;
    [SerializeField] private GameObject storagePanel;
    [SerializeField] private GameObject optionPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Transform battleButtonRoot;
    [SerializeField] private Transform characterButtonRoot;
    [SerializeField] private Transform shopButtonRoot;
    [SerializeField] private Transform researchButtonRoot;
    [SerializeField] private Transform storageButtonRoot;

    private Button battleButton;
    private Button characterButton;
    private Button shopButton;
    private Button researchButton;
    private Button storageButton;

    private void Awake()
    {
        battleButton = TownButtonBinder.Bind(battleButtonRoot, OpenBattle, this);
        characterButton = TownButtonBinder.Bind(characterButtonRoot, OpenCharacter, this);
        shopButton = TownButtonBinder.Bind(shopButtonRoot, OpenShop, this);
        researchButton = TownButtonBinder.Bind(researchButtonRoot, OpenResearch, this);
        storageButton = TownButtonBinder.Bind(storageButtonRoot, OpenStorage, this);
    }

    private void Start()
    {
        OpenBattle();
    }

    private void OnDestroy()
    {
        TownButtonBinder.Unbind(battleButton, OpenBattle);
        TownButtonBinder.Unbind(characterButton, OpenCharacter);
        TownButtonBinder.Unbind(shopButton, OpenShop);
        TownButtonBinder.Unbind(researchButton, OpenResearch);
        TownButtonBinder.Unbind(storageButton, OpenStorage);
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

internal static class TownButtonBinder
{
    public static Button Bind(Transform buttonRoot, UnityAction listener, Object context)
    {
        if (buttonRoot == null)
            return null;

        Button button = buttonRoot.GetComponentInChildren<Button>(true);
        if (button == null)
        {
            Debug.LogError($"[Town] {buttonRoot.name} does not contain a Button component.", context);
            return null;
        }

        button.onClick.AddListener(listener);
        return button;
    }

    public static void Unbind(Button button, UnityAction listener)
    {
        if (button != null)
            button.onClick.RemoveListener(listener);
    }
}
