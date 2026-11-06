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
    [SerializeField] private Transform optionButtonRoot;

    [Header("Panel Controllers")]
    [Tooltip("ITownPanel controllers under [System]/TownUI. Each is notified when its Panel opens or closes.")]
    [SerializeField] private MonoBehaviour[] panelControllers = new MonoBehaviour[0];

    private GameObject currentPanel;
    private Button battleButton;
    private Button characterButton;
    private Button shopButton;
    private Button researchButton;
    private Button storageButton;
    private Button optionButton;

    private void Awake()
    {
        battleButton = TownButtonBinder.Bind(battleButtonRoot, OpenBattle, this);
        characterButton = TownButtonBinder.Bind(characterButtonRoot, OpenCharacter, this);
        shopButton = TownButtonBinder.Bind(shopButtonRoot, OpenShop, this);
        researchButton = TownButtonBinder.Bind(researchButtonRoot, OpenResearch, this);
        storageButton = TownButtonBinder.Bind(storageButtonRoot, OpenStorage, this);
        // Settings uses the scene's existing persistent OpenOption listener.
        optionButton = optionButtonRoot != null ? optionButtonRoot.GetComponentInChildren<Button>(true) : null;
    }

    private void Start()
    {
        OpenBattle();
    }

    private void OnValidate()
    {
        foreach (MonoBehaviour behaviour in panelControllers)
            if (behaviour != null && behaviour is not ITownPanel)
                Debug.LogWarning($"[Town] {behaviour.GetType().Name} does not implement ITownPanel.", this);
    }

    private void OnDestroy()
    {
        NotifyPanelControllers(currentPanel, null);
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
        TownButtonBinder.Select(targetPanel == battlePanel ? battleButton :
            targetPanel == characterPanel ? characterButton : targetPanel == shopPanel ? shopButton :
            targetPanel == researchPanel ? researchButton : targetPanel == storagePanel ? storageButton :
            targetPanel == optionPanel ? optionButton : null,
            battleButton, characterButton, shopButton, researchButton, storageButton, optionButton);
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

        GameObject previousPanel = currentPanel;
        currentPanel = targetPanel;
        NotifyPanelControllers(previousPanel, targetPanel);
    }

    // Mirrors OnEnable/OnDisable: only an actual panel change notifies, reopening the same panel does not.
    private void NotifyPanelControllers(GameObject hiddenPanel, GameObject shownPanel)
    {
        if (hiddenPanel == shownPanel)
            return;

        foreach (MonoBehaviour behaviour in panelControllers)
        {
            if (behaviour == null || behaviour is not ITownPanel controller || controller.Panel == null)
                continue;
            if (controller.Panel == hiddenPanel)
                controller.OnPanelHidden();
            if (controller.Panel == shownPanel)
                controller.OnPanelShown();
        }
    }
}

internal static class TownButtonBinder
{
    public static void Select(Button selected, params Button[] group)
    {
        foreach (Button button in group)
            if (button != null) GetVisual(button).SetSelected(button == selected);
    }

    public static MenuButtonVisual GetVisual(Button button)
    {
        var visual = button.GetComponent<MenuButtonVisual>();
        if (visual != null) return visual;
        visual = button.gameObject.AddComponent<MenuButtonVisual>();
        visual.Initialize(button.targetGraphic as Image, button.GetComponentInChildren<TMPro.TMP_Text>(true));
        return visual;
    }

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
