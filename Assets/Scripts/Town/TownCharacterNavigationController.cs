using UnityEngine;
using UnityEngine.UI;

public sealed class TownCharacterNavigationController : MonoBehaviour
{
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
    [SerializeField] private Transform[] equipPartButtonRoots;
    [SerializeField] private Transform equipBackButtonRoot;

    private Button statButton;
    private Button equipButton;
    private Button skillButton;
    private Button mainStatButton;
    private Button subStatButton;
    private Button elementPowerButton;
    private Button elementResistButton;
    private Button[] equipPartButtons;
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
        equipBackButton = TownButtonBinder.Bind(equipBackButtonRoot, OpenEquipMain, this);

        equipPartButtons = new Button[equipPartButtonRoots.Length];
        for (int i = 0; i < equipPartButtonRoots.Length; i++)
            equipPartButtons[i] = TownButtonBinder.Bind(equipPartButtonRoots[i], OpenEquipSelection, this);

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
        TownButtonBinder.Unbind(equipBackButton, OpenEquipMain);

        if (equipPartButtons == null)
            return;

        foreach (Button button in equipPartButtons)
            TownButtonBinder.Unbind(button, OpenEquipSelection);
    }

    public void OpenStat()
    {
        ShowCharacterScreen(statPanel);
    }

    public void OpenEquip()
    {
        ShowCharacterScreen(equipPanel);
        OpenEquipMain();
    }

    public void OpenSkill()
    {
        ShowCharacterScreen(skillPanel);
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
    }

    public void OpenEquipSelection()
    {
        SetActive(equipMainPanel, false);
        SetActive(equipSelectPartsPanel, true);
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
