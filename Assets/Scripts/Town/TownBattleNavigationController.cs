using UnityEngine;
using UnityEngine.UI;

public sealed class TownBattleNavigationController : MonoBehaviour
{
    private enum BattleType
    {
        Search,
        Attack,
        Defense,
        Conquest,
        Raid
    }

    [SerializeField] private TownTitleController titleController;

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
    private BattleType selectedBattleType;

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
        selectedBattleType = BattleType.Search;
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
        SetActive(stageSelectPanel, true);
        OpenQuestDescription();
        titleController?.SetTitle(GetSelectedBattleTitle());
    }

    public void SelectSearch() => selectedBattleType = BattleType.Search;

    public void SelectAttack() => selectedBattleType = BattleType.Attack;

    public void SelectDefense() => selectedBattleType = BattleType.Defense;

    public void SelectConquest() => selectedBattleType = BattleType.Conquest;

    public void SelectRaid() => selectedBattleType = BattleType.Raid;

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
        switch (selectedBattleType)
        {
            case BattleType.Attack:
                return "공격전";
            case BattleType.Defense:
                return "수성전";
            case BattleType.Conquest:
                return "공략전";
            case BattleType.Raid:
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
