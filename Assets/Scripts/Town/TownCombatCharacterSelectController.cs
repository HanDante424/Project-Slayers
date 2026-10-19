using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class TownCombatCharacterSelectController : MonoBehaviour
{
    [Serializable] public sealed class CharacterOption
    {
        public PlayableCharacterData character;
        public GameObject playerPrefab;
        public Button button;
        public Image icon;
    }
    [Serializable] public struct StatField { public StatType stat; public TMP_Text text; }
    [SerializeField] private TownBattleNavigationController navigationController;
    [SerializeField] private TownStageSelectController stages;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private TownTitleController title;
    [SerializeField] private GameObject stagePanel;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private Button stageConfirm;
    [SerializeField] private Button characterConfirm;
    [SerializeField] private Button backButton;
    [SerializeField] private CharacterOption ern;
    [SerializeField] private CharacterOption mayhem;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image fullBodyImage;
    [SerializeField] private StatField[] statFields;
    [SerializeField] private TMP_Text[] equipmentTexts;
    private CharacterOption selected;
    private MapData selectedMap;
    private CharacterStats preview;
    private bool loading;
    public PlayableCharacterData SelectedCharacter => selected?.character;
    public MapData SelectedMap => selectedMap;
    public const int PreviewLevel = 1;

    private void Awake()
    {
        stageConfirm.onClick.AddListener(OpenSelectedStage);
        characterConfirm.onClick.AddListener(EnterCombat);
        backButton.onClick.AddListener(Back);
        ern.button.onClick.AddListener(SelectErn);
        mayhem.button.onClick.AddListener(SelectMayhem);
        SetIcon(ern); SetIcon(mayhem);
    }
    private void OnDestroy()
    {
        if (stageConfirm != null) stageConfirm.onClick.RemoveListener(OpenSelectedStage);
        if (characterConfirm != null) characterConfirm.onClick.RemoveListener(EnterCombat);
        if (backButton != null) backButton.onClick.RemoveListener(Back);
        if (ern?.button != null) ern.button.onClick.RemoveListener(SelectErn);
        if (mayhem?.button != null) mayhem.button.onClick.RemoveListener(SelectMayhem);
        if (preview != null) Destroy(preview.gameObject);
    }
    public void OpenSelectedStage()
    {
        MapData map = stages.SelectedMapData;
        if (map == null || stageManager == null || !stageManager.IsStageUnlocked(map)) return;
        selectedMap = map; loading = false;
        stagePanel.SetActive(false);
        characterPanel.SetActive(true);
        SelectMayhem();
        title?.SetTitle("캐릭터 선택");
    }
    public void SelectErn() => Select(ern);
    public void SelectMayhem() => Select(mayhem);
    private void Select(CharacterOption option)
    {
        selected = option;
        var definition = option?.character;
        bool valid = definition != null && definition.statsData != null && option.playerPrefab != null;
        characterConfirm.interactable = valid && selectedMap != null && !loading;
        characterNameText.text = definition != null ? definition.characterName : "캐릭터 정보 없음";
        levelText.text = $"레벨 {PreviewLevel}";
        fullBodyImage.sprite = definition != null ? definition.fullBodyImage : null;
        fullBodyImage.enabled = fullBodyImage.sprite != null;
        if (valid)
        {
            TownCharacterSelection.Select(definition, PreviewLevel);
            if (preview == null) preview = new GameObject("Character stat preview", typeof(CharacterStats)).GetComponent<CharacterStats>();
            preview.Initialize(definition.statsData, PreviewLevel);
        }
        foreach (var field in statFields)
            if (field.text != null) field.text.text = valid ? FormatStat(field.stat, preview.GetFinalStat(field.stat)) : "—";
        foreach (var text in equipmentTexts) if (text != null) text.text = "미등록";
        TownButtonBinder.Select(option?.button, ern.button, mayhem.button);
    }
    public static string FormatStat(StatType stat, float value)
    {
        return CharacterStatPanelView.FormatStat(stat, value);
    }
    public void Back()
    {
        characterPanel.SetActive(false); stagePanel.SetActive(true);
        var navigation = navigationController != null ? navigationController : GetComponent<TownBattleNavigationController>();
        navigation?.OpenBasicDescription();
        title?.SetTitle(selectedMap != null ? GetMapTitle(selectedMap.mapType) : "전투");
    }
    private static string GetMapTitle(MapType type) => type switch
    { MapType.Search => "수색전", MapType.Attack => "공격전", MapType.Defense => "수성전", MapType.Conquest => "공략전", MapType.Raid => "레이드", _ => "전투" };
    public void EnterCombat()
    {
        if (loading || !characterPanel.activeInHierarchy || selectedMap == null || stageManager == null ||
            !stageManager.IsStageUnlocked(selectedMap) || selected == null ||
            !Application.CanStreamedLevelBeLoaded("Dev_Combat")) return;
        if (!CombatEntrySelection.TrySet(selectedMap, selected.character, selected.playerPrefab, PreviewLevel)) return;
        loading = true; characterConfirm.interactable = false;
        SceneManager.LoadScene("Dev_Combat");
    }
    private static void SetIcon(CharacterOption option)
    {
        if (option?.icon == null) return;
        option.icon.sprite = option.character != null ? option.character.thumbnail : null;
        option.icon.enabled = option.icon.sprite != null;
    }
}
