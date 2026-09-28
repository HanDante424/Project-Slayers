using UnityEngine;
using UnityEngine.UI;

public sealed class TownCharacterNavigationController : MonoBehaviour
{
    [SerializeField] private TownTitleController titleController;

    [Header("Character Screens")]
    [SerializeField] private GameObject statPanel;
    [SerializeField] private GameObject equipPanel;
    [SerializeField] private GameObject skillPanel;

    [Header("Stat Categories")]
    [SerializeField] private GameObject mainStatPanel;
    [SerializeField] private GameObject subStatPanel;
    [SerializeField] private GameObject elementPowerStatPanel;
    [SerializeField] private GameObject elementResistStatPanel;

    [Header("Equipment Screens")]
    [SerializeField] private GameObject equipMainPanel;
    [SerializeField] private GameObject equipSelectPartsPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Transform statButtonRoot;
    [SerializeField] private Transform equipButtonRoot;
    [SerializeField] private Transform skillButtonRoot;

    [Header("Stat Category Buttons")]
    [SerializeField] private Transform mainStatButtonRoot;
    [SerializeField] private Transform subStatButtonRoot;
    [SerializeField] private Transform elementPowerButtonRoot;
    [SerializeField] private Transform elementResistButtonRoot;

    [Header("Equipment Navigation Buttons")]
    [SerializeField] private Transform beltButtonRoot;
    [SerializeField] private Transform braceletButtonRoot;
    [SerializeField] private Transform necklaceButtonRoot;
    [SerializeField] private Transform glovesButtonRoot;
    [SerializeField] private Transform hairAccessoryButtonRoot;
    [SerializeField] private Transform backAccessoryButtonRoot;
    [SerializeField] private Transform earringsButtonRoot;
    [SerializeField] private Transform ringsButtonRoot;
    [SerializeField] private Transform badgeButtonRoot;
    [SerializeField] private Transform equipBackButtonRoot;

    private Button statButton;
    private Button equipButton;
    private Button skillButton;
    private Button mainStatButton;
    private Button subStatButton;
    private Button elementPowerButton;
    private Button elementResistButton;
    private Button beltButton;
    private Button braceletButton;
    private Button necklaceButton;
    private Button glovesButton;
    private Button hairAccessoryButton;
    private Button backAccessoryButton;
    private Button earringsButton;
    private Button ringsButton;
    private Button badgeButton;
    private Button equipBackButton;

    private void OnEnable()
    {
        statButton = TownButtonBinder.Bind(statButtonRoot, OpenStat, this);
        equipButton = TownButtonBinder.Bind(equipButtonRoot, OpenEquip, this);
        skillButton = TownButtonBinder.Bind(skillButtonRoot, OpenSkill, this);
        mainStatButton = TownButtonBinder.Bind(mainStatButtonRoot, ToggleMainStats, this);
        subStatButton = TownButtonBinder.Bind(subStatButtonRoot, ToggleSubStats, this);
        elementPowerButton = TownButtonBinder.Bind(elementPowerButtonRoot, ToggleElementPowerStats, this);
        elementResistButton = TownButtonBinder.Bind(elementResistButtonRoot, ToggleElementResistStats, this);
        beltButton = TownButtonBinder.Bind(beltButtonRoot, OpenBeltSelection, this);
        braceletButton = TownButtonBinder.Bind(braceletButtonRoot, OpenBraceletSelection, this);
        necklaceButton = TownButtonBinder.Bind(necklaceButtonRoot, OpenNecklaceSelection, this);
        glovesButton = TownButtonBinder.Bind(glovesButtonRoot, OpenGlovesSelection, this);
        hairAccessoryButton = TownButtonBinder.Bind(hairAccessoryButtonRoot, OpenHairAccessorySelection, this);
        backAccessoryButton = TownButtonBinder.Bind(backAccessoryButtonRoot, OpenBackAccessorySelection, this);
        earringsButton = TownButtonBinder.Bind(earringsButtonRoot, OpenEarringsSelection, this);
        ringsButton = TownButtonBinder.Bind(ringsButtonRoot, OpenRingsSelection, this);
        badgeButton = TownButtonBinder.Bind(badgeButtonRoot, OpenBadgeSelection, this);
        equipBackButton = TownButtonBinder.Bind(equipBackButtonRoot, OpenEquipMain, this);

        OpenStat();
    }

    private void OnDisable()
    {
        TownButtonBinder.Unbind(statButton, OpenStat);
        TownButtonBinder.Unbind(equipButton, OpenEquip);
        TownButtonBinder.Unbind(skillButton, OpenSkill);
        TownButtonBinder.Unbind(mainStatButton, ToggleMainStats);
        TownButtonBinder.Unbind(subStatButton, ToggleSubStats);
        TownButtonBinder.Unbind(elementPowerButton, ToggleElementPowerStats);
        TownButtonBinder.Unbind(elementResistButton, ToggleElementResistStats);
        TownButtonBinder.Unbind(beltButton, OpenBeltSelection);
        TownButtonBinder.Unbind(braceletButton, OpenBraceletSelection);
        TownButtonBinder.Unbind(necklaceButton, OpenNecklaceSelection);
        TownButtonBinder.Unbind(glovesButton, OpenGlovesSelection);
        TownButtonBinder.Unbind(hairAccessoryButton, OpenHairAccessorySelection);
        TownButtonBinder.Unbind(backAccessoryButton, OpenBackAccessorySelection);
        TownButtonBinder.Unbind(earringsButton, OpenEarringsSelection);
        TownButtonBinder.Unbind(ringsButton, OpenRingsSelection);
        TownButtonBinder.Unbind(badgeButton, OpenBadgeSelection);
        TownButtonBinder.Unbind(equipBackButton, OpenEquipMain);
    }

    public void OpenStat()
    {
        ShowCharacterScreen(statPanel);
        titleController?.SetTitle("능력치");
    }

    public void OpenEquip()
    {
        ShowCharacterScreen(equipPanel);
        OpenEquipMain();
    }

    public void OpenSkill()
    {
        ShowCharacterScreen(skillPanel);
        titleController?.SetTitle("스킬");
    }

    public void ToggleMainStats()
    {
        TogglePanel(mainStatPanel);
    }

    public void ToggleSubStats()
    {
        TogglePanel(subStatPanel);
    }

    public void ToggleElementPowerStats()
    {
        TogglePanel(elementPowerStatPanel);
    }

    public void ToggleElementResistStats()
    {
        TogglePanel(elementResistStatPanel);
    }

    public void OpenEquipMain()
    {
        SetActive(equipMainPanel, true);
        SetActive(equipSelectPartsPanel, false);
        titleController?.SetTitle("장비");
    }

    public void OpenBeltSelection() => OpenEquipSelection("장비 - 벨트");

    public void OpenBraceletSelection() => OpenEquipSelection("장비 - 팔찌");

    public void OpenNecklaceSelection() => OpenEquipSelection("장비 - 목걸이");

    public void OpenGlovesSelection() => OpenEquipSelection("장비 - 장갑");

    public void OpenHairAccessorySelection() => OpenEquipSelection("장비 - 머리 장식");

    public void OpenBackAccessorySelection() => OpenEquipSelection("장비 - 등 장식");

    public void OpenEarringsSelection() => OpenEquipSelection("장비 - 귀걸이");

    public void OpenRingsSelection() => OpenEquipSelection("장비 - 반지");

    public void OpenBadgeSelection() => OpenEquipSelection("장비 - 문양");

    private void OpenEquipSelection(string title)
    {
        SetActive(equipMainPanel, false);
        SetActive(equipSelectPartsPanel, true);
        titleController?.SetTitle(title);
    }

    private void ShowCharacterScreen(GameObject targetPanel)
    {
        SetActive(statPanel, statPanel == targetPanel);
        SetActive(equipPanel, equipPanel == targetPanel);
        SetActive(skillPanel, skillPanel == targetPanel);
    }

    private static void TogglePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    private static void SetActive(GameObject panel, bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }
}
