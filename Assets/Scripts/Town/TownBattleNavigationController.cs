using UnityEngine;
using UnityEngine.UI;

public sealed class TownBattleNavigationController : MonoBehaviour
{
    [SerializeField] private TownTitleController titleController;

    [SerializeField] private TownStageSelectController stageSelectController;

    [Header("Battle Screens")]
    [SerializeField] private GameObject battleSelectPanel;
    [SerializeField] private GameObject stageSelectPanel;

    [Header("Stage Description Tabs")]
    [SerializeField] private GameObject basicDescriptionPanel;
    [SerializeField] private GameObject monsterDescriptionPanel;
    [SerializeField] private GameObject rewardDescriptionPanel;
    [SerializeField] private GameObject questDescriptionPanel;

    [Header("Buttons")]
    [SerializeField] private Transform searchButtonRoot;
    [SerializeField] private Transform attackButtonRoot;
    [SerializeField] private Transform defenseButtonRoot;
    [SerializeField] private Transform conquestButtonRoot;
    [SerializeField] private Transform raidButtonRoot;
    [SerializeField] private Transform selectButtonRoot;
    [SerializeField] private Transform backButtonRoot;
    [SerializeField] private Transform basicButtonRoot;
    [SerializeField] private Transform monsterButtonRoot;
    [SerializeField] private Transform rewardButtonRoot;
    [SerializeField] private Transform questButtonRoot;

    private Button searchButton;
    private Button attackButton;
    private Button defenseButton;
    private Button conquestButton;
    private Button raidButton;
    private Button selectButton;
    private Button backButton;
    private Button basicButton;
    private Button monsterButton;
    private Button rewardButton;
    private Button questButton;
    private MapType selectedMapType;

    private void OnEnable()
    {
        searchButton = TownButtonBinder.Bind(searchButtonRoot, SelectSearch, this);
        attackButton = TownButtonBinder.Bind(attackButtonRoot, SelectAttack, this);
        defenseButton = TownButtonBinder.Bind(defenseButtonRoot, SelectDefense, this);
        conquestButton = TownButtonBinder.Bind(conquestButtonRoot, SelectConquest, this);
        raidButton = TownButtonBinder.Bind(raidButtonRoot, SelectRaid, this);
        selectButton = TownButtonBinder.Bind(selectButtonRoot, OpenStageSelect, this);
        backButton = TownButtonBinder.Bind(backButtonRoot, OpenBattleSelect, this);
        basicButton = TownButtonBinder.Bind(basicButtonRoot, OpenBasicDescription, this);
        monsterButton = TownButtonBinder.Bind(monsterButtonRoot, OpenMonsterDescription, this);
        rewardButton = TownButtonBinder.Bind(rewardButtonRoot, OpenRewardDescription, this);
        questButton = TownButtonBinder.Bind(questButtonRoot, OpenQuestDescription, this);
        selectedMapType = MapType.Search;
        OpenBattleSelect();
    }

    private void OnDisable()
    {
        TownButtonBinder.Unbind(searchButton, SelectSearch);
        TownButtonBinder.Unbind(attackButton, SelectAttack);
        TownButtonBinder.Unbind(defenseButton, SelectDefense);
        TownButtonBinder.Unbind(conquestButton, SelectConquest);
        TownButtonBinder.Unbind(raidButton, SelectRaid);
        TownButtonBinder.Unbind(selectButton, OpenStageSelect);
        TownButtonBinder.Unbind(backButton, OpenBattleSelect);
        TownButtonBinder.Unbind(basicButton, OpenBasicDescription);
        TownButtonBinder.Unbind(monsterButton, OpenMonsterDescription);
        TownButtonBinder.Unbind(rewardButton, OpenRewardDescription);
        TownButtonBinder.Unbind(questButton, OpenQuestDescription);
    }

    public void OpenBattleSelect()
    {
        SetActive(battleSelectPanel, true);
        SetActive(stageSelectPanel, false);
        titleController?.SetTitle("전투");
    }

    public void OpenStageSelect()
    {
        SetActive(battleSelectPanel, false);
        stageSelectController?.Show(selectedMapType);
        SetActive(stageSelectPanel, true);
        OpenBasicDescription();
        titleController?.SetTitle(GetSelectedBattleTitle());
    }

    public void SelectSearch() => selectedMapType = MapType.Search;

    public void SelectAttack() => selectedMapType = MapType.Attack;

    public void SelectDefense() => selectedMapType = MapType.Defense;

    public void SelectConquest() => selectedMapType = MapType.Conquest;

    public void SelectRaid() => selectedMapType = MapType.Raid;

    public void OpenBasicDescription()
    {
        ShowDescription(basicDescriptionPanel);
    }

    public void OpenMonsterDescription()
    {
        ShowDescription(monsterDescriptionPanel);
    }

    public void OpenRewardDescription()
    {
        ShowDescription(rewardDescriptionPanel);
    }

    public void OpenQuestDescription()
    {
        ShowDescription(questDescriptionPanel);
    }

    private void ShowDescription(GameObject targetPanel)
    {
        SetActive(basicDescriptionPanel, basicDescriptionPanel == targetPanel);
        SetActive(monsterDescriptionPanel, monsterDescriptionPanel == targetPanel);
        SetActive(rewardDescriptionPanel, rewardDescriptionPanel == targetPanel);
        SetActive(questDescriptionPanel, questDescriptionPanel == targetPanel);
    }

    private string GetSelectedBattleTitle()
    {
        switch (selectedMapType)
        {
            case MapType.Attack:
                return "공격전";
            case MapType.Defense:
                return "수성전";
            case MapType.Conquest:
                return "공략전";
            case MapType.Raid:
                return "레이드";
            default:
                return "수색전";
        }
    }

    private static void SetActive(GameObject panel, bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }
}
