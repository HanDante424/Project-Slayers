using System;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TownCharacterStatsController : MonoBehaviour
{
    [Serializable]
    private sealed class CharacterButton
    {
        public PlayableCharacterData character;
        public Button button;
        public Image icon;
    }

    [Header("Character List")]
    [SerializeField] private CharacterButton mayhem;
    [SerializeField] private CharacterButton ern;
    [SerializeField] private PlayableCharacterData defaultCharacter;
    [SerializeField, Min(1)] private int defaultLevel = 1;
    [SerializeField] private CharacterStatPanelView detailView;
    [SerializeField] private CharacterStatPanelView overviewView;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image fullBodyImage;
    [SerializeField] private TMP_Text maximumLevelText, experienceText;
    [SerializeField] private Slider experienceGauge;
    [SerializeField] private Transform openDetailButtonRoot;
    [SerializeField] private Transform closeDetailButtonRoot;

    [SerializeField] private CharacterButton[] additionalCharacters = Array.Empty<CharacterButton>();
    [SerializeField] private TownCharacterOverviewView elementOverview;
    [SerializeField] private TownCharacterDataSource dataSource;
    public PlayableCharacterData CurrentCharacter { get; private set; }
    public CharacterStats CurrentStats { get; private set; }
    public event Action Refreshed;
    private readonly Dictionary<PlayableCharacterData, CharacterStats> runtimeStats = new();
    private readonly Dictionary<Button, UnityAction> additionalListeners = new();
    private PlayableCharacterData previewCharacter;
    private CharacterStats subscribedStats;
    private CharacterStats preview;
    private Button openDetailButton;
    private Button closeDetailButton;

    private void OnEnable()
    {
        TownCharacterSelection.Changed += RefreshCharacter;
        BindCharacterButton(mayhem, SelectMayhem);
        BindCharacterButton(ern, SelectErn);
        foreach (var option in additionalCharacters)
        {
            if (option?.button == null || additionalListeners.ContainsKey(option.button)) continue;
            UnityAction action = () => SelectCharacter(option);
            BindCharacterButton(option, action); additionalListeners[option.button] = action;
        }
        if (dataSource != null) dataSource.Changed += RefreshCharacter;
        openDetailButton = TownButtonBinder.Bind(openDetailButtonRoot, OpenDetails, this);
        closeDetailButton = TownButtonBinder.Bind(closeDetailButtonRoot, CloseDetails, this);
        CloseDetails();
        if (TownCharacterSelection.Character == null) TownCharacterSelection.Select(defaultCharacter, defaultLevel);
        RefreshCharacter();
    }

    private void OnDisable()
    {
        TownCharacterSelection.Changed -= RefreshCharacter;
        if (dataSource != null) dataSource.Changed -= RefreshCharacter;
        foreach (var pair in additionalListeners) TownButtonBinder.Unbind(pair.Key, pair.Value);
        additionalListeners.Clear();
        SubscribeStats(null);
        TownButtonBinder.Unbind(mayhem?.button, SelectMayhem);
        TownButtonBinder.Unbind(ern?.button, SelectErn);
        TownButtonBinder.Unbind(openDetailButton, OpenDetails);
        TownButtonBinder.Unbind(closeDetailButton, CloseDetails);
        CloseDetails();
    }

    private void OnDestroy()
    {
        if (detailView != null) detailView.Bind(null);
        if (overviewView != null) overviewView.Bind(null);
        elementOverview?.Bind(null);
        SubscribeStats(null);
        if (preview != null) Destroy(preview.gameObject);
    }

    public void RefreshCharacter()
    {
        SubscribeStats(null);
        PlayableCharacterData character = TownCharacterSelection.Character;
        bool selected = character != null;
        if (!selected) character = defaultCharacter;
        int level = selected ? TownCharacterSelection.Level : Mathf.Max(1, defaultLevel);
        bool valid = character != null && character.statsData != null;
        CurrentCharacter = character;
        CharacterStats bound = null;
        if (character != null) runtimeStats.TryGetValue(character, out bound);
        CharacterStats live = (dataSource != null ? dataSource.GetStats(character) : null) ?? bound;
        if (live != null && (!valid || live.Data != character.statsData)) live = null;
        if (live != null) level = live.Level;

        if (valid && live == null)
        {
            if (preview == null)
            {
                // Configure an inactive instance so its Awake cannot read an unassigned definition.
                var owner = new GameObject("Town character stat preview");
                owner.SetActive(false);
                owner.transform.SetParent(transform, false);
                preview = owner.AddComponent<CharacterStats>();
            }
            if (previewCharacter != character || preview.Data != character.statsData || preview.Level != level)
                preview.Initialize(character.statsData, level);
            previewCharacter = character;
        }
        CurrentStats = valid ? (live != null ? live : preview) : null;
        detailView?.Bind(CurrentStats);
        overviewView?.Bind(CurrentStats);
        elementOverview?.Bind(CurrentStats);
        SubscribeStats(CurrentStats);
        if (characterNameText != null)
            characterNameText.text = character != null ? character.characterName : "캐릭터 정보 없음";
        if (levelText != null)
            levelText.text = valid ? level.ToString() : "—";
        // No character XP/max-level progression exists yet; never present the authored sample as saved progress.
        if (maximumLevelText != null) maximumLevelText.text = string.Empty;
        if (experienceText != null) experienceText.text = string.Empty;
        if (experienceGauge != null) { experienceGauge.SetValueWithoutNotify(experienceGauge.minValue); experienceGauge.interactable = false; }
        if (fullBodyImage != null)
        {
            fullBodyImage.sprite = character != null ? character.fullBodyImage : null;
            fullBodyImage.enabled = fullBodyImage.sprite != null;
        }
        TownButtonBinder.Select(character == mayhem?.character ? mayhem?.button :
            character == ern?.character ? ern?.button : null, mayhem?.button, ern?.button);
        foreach (var option in additionalCharacters) option?.button?.GetComponent<MenuButtonVisual>()?.SetSelected(option.character == character);
        Refreshed?.Invoke();
    }

    // A caller must identify the owning character; a different character can never inherit these modifiers.
    public void BindRuntimeStats(PlayableCharacterData character, CharacterStats stats)
    {
        if (character == null) return;
        if (stats == null) runtimeStats.Remove(character);
        else if (stats.Data == character.statsData) runtimeStats[character] = stats;
        if (CurrentCharacter == character) RefreshCharacter();
    }
    private void SubscribeStats(CharacterStats stats)
    {
        if (subscribedStats != null) subscribedStats.StatsChanged -= OnStatsChanged;
        subscribedStats = isActiveAndEnabled ? stats : null;
        if (subscribedStats != null) subscribedStats.StatsChanged += OnStatsChanged;
    }
    private void OnStatsChanged()
    {
        if (levelText != null && CurrentStats != null) levelText.text = CurrentStats.Level.ToString();
        Refreshed?.Invoke();
    }

    public void SelectMayhem() => SelectCharacter(mayhem);
    public void SelectErn() => SelectCharacter(ern);

    private void SelectCharacter(CharacterButton option)
    {
        if (option?.character != null)
            TownCharacterSelection.Select(option.character, Mathf.Max(1, defaultLevel));
    }

    private static void BindCharacterButton(CharacterButton option, UnityEngine.Events.UnityAction action)
    {
        if (option?.button == null) return;
        if (option.icon != null)
        {
            option.icon.sprite = option.character != null ? option.character.thumbnail : null;
            option.icon.enabled = option.icon.sprite != null;
        }
        option.button.onClick.AddListener(action);
    }

    public void OpenDetails()
    {
        if (detailView == null) return;
        detailView.gameObject.SetActive(true);
        detailView.Refresh();
    }

    public void CloseDetails()
    {
        if (detailView != null) detailView.gameObject.SetActive(false);
    }
}
