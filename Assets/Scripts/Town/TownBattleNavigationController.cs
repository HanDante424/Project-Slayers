using UnityEngine;
using UnityEngine.UI;

public sealed class TownBattleNavigationController : MonoBehaviour
{
    [Header("Battle Screens")]
    [SerializeField] private GameObject battleSelectPanel;
    [SerializeField] private GameObject stageSelectPanel;

    [Header("Stage Description Tabs")]
    [SerializeField] private GameObject basicDescriptionPanel;
    [SerializeField] private GameObject monsterDescriptionPanel;
    [SerializeField] private GameObject rewardDescriptionPanel;
    [SerializeField] private GameObject questDescriptionPanel;

    [Header("Buttons")]
    [SerializeField] private Transform selectButtonRoot;
    [SerializeField] private Transform backButtonRoot;
    [SerializeField] private Transform basicButtonRoot;
    [SerializeField] private Transform monsterButtonRoot;
    [SerializeField] private Transform rewardButtonRoot;
    [SerializeField] private Transform questButtonRoot;

    private Button selectButton;
    private Button backButton;
    private Button basicButton;
    private Button monsterButton;
    private Button rewardButton;
    private Button questButton;

    private void OnEnable()
    {
        selectButton = TownButtonBinder.Bind(selectButtonRoot, OpenStageSelect, this);
        backButton = TownButtonBinder.Bind(backButtonRoot, OpenBattleSelect, this);
        basicButton = TownButtonBinder.Bind(basicButtonRoot, OpenBasicDescription, this);
        monsterButton = TownButtonBinder.Bind(monsterButtonRoot, OpenMonsterDescription, this);
        rewardButton = TownButtonBinder.Bind(rewardButtonRoot, OpenRewardDescription, this);
        questButton = TownButtonBinder.Bind(questButtonRoot, OpenQuestDescription, this);
        OpenBattleSelect();
    }

    private void OnDisable()
    {
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
    }

    public void OpenStageSelect()
    {
        SetActive(battleSelectPanel, false);
        SetActive(stageSelectPanel, true);
        OpenQuestDescription();
    }

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

    private static void SetActive(GameObject panel, bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }
}
