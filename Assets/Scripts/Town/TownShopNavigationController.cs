using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class TownShopNavigationController : MonoBehaviour
{
    [Header("Shop Screens")]
    [SerializeField] private GameObject generalExchangePanel;
    [SerializeField] private GameObject blackMarketPanel;
    [SerializeField] private GameObject blackSmithPanel;

    [Header("Blacksmith Screens")]
    [SerializeField] private GameObject blackSmithPartsPanel;
    [SerializeField] private GameObject blackSmithMainPanel;
    [SerializeField] private TMP_Text blackSmithMaterialLabel;
    [SerializeField] private TMP_Text blackSmithCostLabel;

    [Header("Shop Buttons")]
    [SerializeField] private Transform generalExchangeButtonRoot;
    [SerializeField] private Transform blackMarketButtonRoot;
    [SerializeField] private Transform blackSmithButtonRoot;
    [SerializeField] private Transform buyButtonRoot;
    [SerializeField] private Transform sellButtonRoot;
    [SerializeField] private Transform craftButtonRoot;
    [SerializeField] private Transform enhanceButtonRoot;
    [SerializeField] private Transform questButtonRoot;

    [Header("Blacksmith Navigation Buttons")]
    [SerializeField] private Transform[] blackSmithPartButtonRoots;

    private Button generalExchangeButton;
    private Button blackMarketButton;
    private Button blackSmithButton;
    private Button craftButton;
    private Button enhanceButton;
    private Button[] blackSmithPartButtons;

    private void OnEnable()
    {
        generalExchangeButton = TownButtonBinder.Bind(generalExchangeButtonRoot, OpenGeneralExchange, this);
        blackMarketButton = TownButtonBinder.Bind(blackMarketButtonRoot, OpenBlackMarket, this);
        blackSmithButton = TownButtonBinder.Bind(blackSmithButtonRoot, OpenBlackSmith, this);
        craftButton = TownButtonBinder.Bind(craftButtonRoot, OpenCraft, this);
        enhanceButton = TownButtonBinder.Bind(enhanceButtonRoot, OpenEnhance, this);

        blackSmithPartButtons = new Button[blackSmithPartButtonRoots.Length];
        for (int i = 0; i < blackSmithPartButtonRoots.Length; i++)
            blackSmithPartButtons[i] = TownButtonBinder.Bind(blackSmithPartButtonRoots[i], OpenBlackSmithMain, this);

        OpenBlackMarket();
    }

    private void OnDisable()
    {
        TownButtonBinder.Unbind(generalExchangeButton, OpenGeneralExchange);
        TownButtonBinder.Unbind(blackMarketButton, OpenBlackMarket);
        TownButtonBinder.Unbind(blackSmithButton, OpenBlackSmith);
        TownButtonBinder.Unbind(craftButton, OpenCraft);
        TownButtonBinder.Unbind(enhanceButton, OpenEnhance);

        if (blackSmithPartButtons == null)
            return;

        foreach (Button button in blackSmithPartButtons)
            TownButtonBinder.Unbind(button, OpenBlackSmithMain);
    }

    public void OpenGeneralExchange()
    {
        ShowShopScreen(generalExchangePanel);
        ShowRetailMenuButtons();
    }

    public void OpenBlackMarket()
    {
        ShowShopScreen(blackMarketPanel);
        ShowRetailMenuButtons();
    }

    public void OpenBlackSmith()
    {
        ShowShopScreen(blackSmithPanel);
        ShowBlackSmithMenuButtons();
        ShowCraftLabels();
        OpenBlackSmithParts();
    }

    public void OpenCraft()
    {
        OpenBlackSmith();
    }

    public void OpenEnhance()
    {
        OpenBlackSmith();
        ShowEnhanceLabels();
    }

    public void OpenBlackSmithParts()
    {
        SetActive(blackSmithPartsPanel, true);
        SetActive(blackSmithMainPanel, false);
    }

    public void OpenBlackSmithMain()
    {
        SetActive(blackSmithPartsPanel, false);
        SetActive(blackSmithMainPanel, true);
    }

    private void ShowShopScreen(GameObject targetPanel)
    {
        SetActive(generalExchangePanel, generalExchangePanel == targetPanel);
        SetActive(blackMarketPanel, blackMarketPanel == targetPanel);
        SetActive(blackSmithPanel, blackSmithPanel == targetPanel);
    }

    private void ShowRetailMenuButtons()
    {
        SetButtonActive(buyButtonRoot, true);
        SetButtonActive(sellButtonRoot, true);
        SetButtonActive(questButtonRoot, true);
        SetButtonActive(craftButtonRoot, false);
        SetButtonActive(enhanceButtonRoot, false);
    }

    private void ShowBlackSmithMenuButtons()
    {
        SetButtonActive(buyButtonRoot, false);
        SetButtonActive(sellButtonRoot, false);
        SetButtonActive(questButtonRoot, true);
        SetButtonActive(craftButtonRoot, true);
        SetButtonActive(enhanceButtonRoot, true);
    }

    private void ShowCraftLabels()
    {
        SetLabel(blackSmithMaterialLabel, "제작 재료");
        SetLabel(blackSmithCostLabel, "제작 비용");
    }

    private void ShowEnhanceLabels()
    {
        SetLabel(blackSmithMaterialLabel, "강화 재료");
        SetLabel(blackSmithCostLabel, "강화 비용");
    }

    private static void SetLabel(TMP_Text label, string value)
    {
        if (label != null)
            label.text = value;
    }

    private static void SetButtonActive(Transform buttonRoot, bool isActive)
    {
        if (buttonRoot != null)
            buttonRoot.gameObject.SetActive(isActive);
    }

    private static void SetActive(GameObject panel, bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }
}
