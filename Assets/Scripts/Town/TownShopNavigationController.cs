using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class TownShopNavigationController : MonoBehaviour
{
    private enum ShopType
    {
        GeneralShop,
        BlackMarket,
        BlackSmith
    }

    private enum BlackSmithFunction
    {
        Craft,
        Enhance
    }

    [SerializeField] private TownTitleController titleController;

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
    [SerializeField] private Transform beltButtonRoot;
    [SerializeField] private Transform braceletButtonRoot;
    [SerializeField] private Transform necklaceButtonRoot;
    [SerializeField] private Transform glovesButtonRoot;
    [SerializeField] private Transform hairAccessoryButtonRoot;
    [SerializeField] private Transform backAccessoryButtonRoot;
    [SerializeField] private Transform earringsButtonRoot;
    [SerializeField] private Transform ringsButtonRoot;
    [SerializeField] private Transform badgeButtonRoot;

    private Button generalExchangeButton;
    private Button blackMarketButton;
    private Button blackSmithButton;
    private Button buyButton;
    private Button sellButton;
    private Button craftButton;
    private Button enhanceButton;
    private Button questButton;
    private Button beltButton;
    private Button braceletButton;
    private Button necklaceButton;
    private Button glovesButton;
    private Button hairAccessoryButton;
    private Button backAccessoryButton;
    private Button earringsButton;
    private Button ringsButton;
    private Button badgeButton;
    private ShopType currentShop;
    private BlackSmithFunction currentBlackSmithFunction;

    private void OnEnable()
    {
        generalExchangeButton = TownButtonBinder.Bind(generalExchangeButtonRoot, OpenGeneralExchange, this);
        blackMarketButton = TownButtonBinder.Bind(blackMarketButtonRoot, OpenBlackMarket, this);
        blackSmithButton = TownButtonBinder.Bind(blackSmithButtonRoot, OpenBlackSmith, this);
        buyButton = TownButtonBinder.Bind(buyButtonRoot, OpenBuy, this);
        sellButton = TownButtonBinder.Bind(sellButtonRoot, OpenSell, this);
        craftButton = TownButtonBinder.Bind(craftButtonRoot, OpenCraft, this);
        enhanceButton = TownButtonBinder.Bind(enhanceButtonRoot, OpenEnhance, this);
        questButton = TownButtonBinder.Bind(questButtonRoot, OpenQuest, this);
        beltButton = TownButtonBinder.Bind(beltButtonRoot, OpenBelt, this);
        braceletButton = TownButtonBinder.Bind(braceletButtonRoot, OpenBracelet, this);
        necklaceButton = TownButtonBinder.Bind(necklaceButtonRoot, OpenNecklace, this);
        glovesButton = TownButtonBinder.Bind(glovesButtonRoot, OpenGloves, this);
        hairAccessoryButton = TownButtonBinder.Bind(hairAccessoryButtonRoot, OpenHairAccessory, this);
        backAccessoryButton = TownButtonBinder.Bind(backAccessoryButtonRoot, OpenBackAccessory, this);
        earringsButton = TownButtonBinder.Bind(earringsButtonRoot, OpenEarrings, this);
        ringsButton = TownButtonBinder.Bind(ringsButtonRoot, OpenRings, this);
        badgeButton = TownButtonBinder.Bind(badgeButtonRoot, OpenBadge, this);

        OpenGeneralExchange();
    }

    private void OnDisable()
    {
        TownButtonBinder.Unbind(generalExchangeButton, OpenGeneralExchange);
        TownButtonBinder.Unbind(blackMarketButton, OpenBlackMarket);
        TownButtonBinder.Unbind(blackSmithButton, OpenBlackSmith);
        TownButtonBinder.Unbind(buyButton, OpenBuy);
        TownButtonBinder.Unbind(sellButton, OpenSell);
        TownButtonBinder.Unbind(craftButton, OpenCraft);
        TownButtonBinder.Unbind(enhanceButton, OpenEnhance);
        TownButtonBinder.Unbind(questButton, OpenQuest);
        TownButtonBinder.Unbind(beltButton, OpenBelt);
        TownButtonBinder.Unbind(braceletButton, OpenBracelet);
        TownButtonBinder.Unbind(necklaceButton, OpenNecklace);
        TownButtonBinder.Unbind(glovesButton, OpenGloves);
        TownButtonBinder.Unbind(hairAccessoryButton, OpenHairAccessory);
        TownButtonBinder.Unbind(backAccessoryButton, OpenBackAccessory);
        TownButtonBinder.Unbind(earringsButton, OpenEarrings);
        TownButtonBinder.Unbind(ringsButton, OpenRings);
        TownButtonBinder.Unbind(badgeButton, OpenBadge);
    }

    public void OpenGeneralExchange()
    {
        currentShop = ShopType.GeneralShop;
        ShowShopScreen(generalExchangePanel);
        ShowRetailMenuButtons();
        OpenBuy();
    }

    public void OpenBlackMarket()
    {
        currentShop = ShopType.BlackMarket;
        ShowShopScreen(blackMarketPanel);
        ShowRetailMenuButtons();
        OpenBuy();
    }

    public void OpenBlackSmith()
    {
        currentShop = ShopType.BlackSmith;
        currentBlackSmithFunction = BlackSmithFunction.Craft;
        ShowShopScreen(blackSmithPanel);
        ShowBlackSmithMenuButtons();
        SelectFunction(craftButton);
        ShowCraftLabels();
        OpenBlackSmithParts();
        titleController?.SetTitle("대장간 - 제작");
    }

    public void OpenBuy()
    {
        SelectFunction(buyButton);
        titleController?.SetTitle($"{GetCurrentShopName()} - 구매");
    }

    public void OpenSell()
    {
        SelectFunction(sellButton);
        titleController?.SetTitle($"{GetCurrentShopName()} - 판매");
    }

    public void OpenCraft()
    {
        OpenBlackSmith();
    }

    public void OpenEnhance()
    {
        OpenBlackSmith();
        SelectFunction(enhanceButton);
        currentBlackSmithFunction = BlackSmithFunction.Enhance;
        ShowEnhanceLabels();
        titleController?.SetTitle("대장간 - 강화");
    }

    public void OpenQuest()
    {
        SelectFunction(questButton);
        titleController?.SetTitle($"{GetCurrentShopName()} - 퀘스트");
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

    public void OpenBelt() => OpenBlackSmithPart("벨트");

    public void OpenBracelet() => OpenBlackSmithPart("팔찌");

    public void OpenNecklace() => OpenBlackSmithPart("목걸이");

    public void OpenGloves() => OpenBlackSmithPart("장갑");

    public void OpenHairAccessory() => OpenBlackSmithPart("머리 장식");

    public void OpenBackAccessory() => OpenBlackSmithPart("등 장식");

    public void OpenEarrings() => OpenBlackSmithPart("귀걸이");

    public void OpenRings() => OpenBlackSmithPart("반지");

    public void OpenBadge() => OpenBlackSmithPart("문양");

    private void ShowShopScreen(GameObject targetPanel)
    {
        TownButtonBinder.Select(targetPanel == generalExchangePanel ? generalExchangeButton :
            targetPanel == blackMarketPanel ? blackMarketButton : blackSmithButton,
            generalExchangeButton, blackMarketButton, blackSmithButton);
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

    private void SelectFunction(Button button) => TownButtonBinder.Select(button,
        buyButton, sellButton, craftButton, enhanceButton, questButton);

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

    private string GetCurrentShopName()
    {
        switch (currentShop)
        {
            case ShopType.GeneralShop:
                return "잡화점";
            case ShopType.BlackSmith:
                return "대장간";
            default:
                return "암상인";
        }
    }

    private void OpenBlackSmithPart(string partName)
    {
        OpenBlackSmithMain();
        string functionName = currentBlackSmithFunction == BlackSmithFunction.Enhance ? "강화" : "제작";
        titleController?.SetTitle($"대장간 - {functionName} - {partName}");
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
