using System;
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
    [SerializeField] private Transform openDetailButtonRoot;
    [SerializeField] private Transform closeDetailButtonRoot;

    private CharacterStats preview;
    private Button openDetailButton;
    private Button closeDetailButton;

    private void OnEnable()
    {
        TownCharacterSelection.Changed += RefreshCharacter;
        BindCharacterButton(mayhem, SelectMayhem);
        BindCharacterButton(ern, SelectErn);
        openDetailButton = TownButtonBinder.Bind(openDetailButtonRoot, OpenDetails, this);
        closeDetailButton = TownButtonBinder.Bind(closeDetailButtonRoot, CloseDetails, this);
        CloseDetails();
        RefreshCharacter();
    }

    private void OnDisable()
    {
        TownCharacterSelection.Changed -= RefreshCharacter;
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
        if (preview != null) Destroy(preview.gameObject);
    }

    public void RefreshCharacter()
    {
        PlayableCharacterData character = TownCharacterSelection.Character;
        bool selected = character != null;
        if (!selected) character = defaultCharacter;
        int level = selected ? TownCharacterSelection.Level : Mathf.Max(1, defaultLevel);
        bool valid = character != null && character.statsData != null;

        if (valid)
        {
            if (preview == null)
            {
                // Configure an inactive instance so its Awake cannot read an unassigned definition.
                var owner = new GameObject("Town character stat preview");
                owner.SetActive(false);
                owner.transform.SetParent(transform, false);
                preview = owner.AddComponent<CharacterStats>();
            }
            preview.Initialize(character.statsData, level);
        }
        detailView?.Bind(valid ? preview : null);
        overviewView?.Bind(valid ? preview : null);
        if (characterNameText != null)
            characterNameText.text = character != null ? character.characterName : "캐릭터 정보 없음";
        if (levelText != null)
            levelText.text = valid ? level.ToString() : "—";
        if (fullBodyImage != null)
        {
            fullBodyImage.sprite = character != null ? character.fullBodyImage : null;
            fullBodyImage.enabled = fullBodyImage.sprite != null;
        }
        TownButtonBinder.Select(character == mayhem?.character ? mayhem?.button :
            character == ern?.character ? ern?.button : null, mayhem?.button, ern?.button);
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
