using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TownCharacterStatsController : MonoBehaviour
{
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
        openDetailButton = TownButtonBinder.Bind(openDetailButtonRoot, OpenDetails, this);
        closeDetailButton = TownButtonBinder.Bind(closeDetailButtonRoot, CloseDetails, this);
        CloseDetails();
        RefreshCharacter();
    }

    private void OnDisable()
    {
        TownCharacterSelection.Changed -= RefreshCharacter;
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
