using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TownStageSelectController : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private TownStageDetailsView detailsView;
    [Header("Existing Layout and Templates")]
    [SerializeField] private RectTransform rankContent;
    [SerializeField] private RectTransform rankButtonTemplate;
    [SerializeField] private GameObject[] rankPlaceholders;
    [SerializeField] private RectTransform stageContent;
    [SerializeField] private RectTransform stageButtonTemplate;
    [SerializeField] private Button selectionButton;
    [Header("Existing Basic Description")]
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text corruptionText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image thumbnail;

    public MapType SelectedMapType { get; private set; } = MapType.Search;
    public int? SelectedRank { get; private set; }
    public MapData SelectedMapData { get; private set; }
    public event Action<MapData> SelectedStageChanged;

    private sealed class Row
    {
        public RectTransform Root;
        public Button Button;
        public MenuButtonVisual Visual;
    }

    private readonly Dictionary<int, Row> rankRows = new();
    private readonly Dictionary<MapData, Row> stageRows = new();

    public void RefreshEnemyDetails() => detailsView?.RefreshEnemies();

    public void RefreshRewardDetails() => detailsView?.RefreshRewards();

    public void Show(MapType mapType)
    {
        SelectedMapType = mapType;
        if (isActiveAndEnabled) Refresh();
    }

    private void OnEnable() => Refresh();

    private void OnDisable()
    {
        ClearRows(rankRows.Values);
        rankRows.Clear();
        ClearStages();
        SelectedRank = null;
    }

    public void Refresh()
    {
        HideTemplates();
        ClearRows(rankRows.Values);
        rankRows.Clear();
        ClearStages();
        SelectedRank = null;
        if (stageManager == null) return;

        List<int> ranks = stageManager.GetAvailableRanks(SelectedMapType);
        foreach (int rank in ranks)
        {
            Row row = CreateRow(rankButtonTemplate, rankContent, $"Rank_{rank}", rank.ToString());
            if (row == null) continue;
            int capturedRank = rank;
            row.Button.onClick.AddListener(() => SelectRank(capturedRank));
            rankRows.Add(rank, row);
        }
        if (ranks.Count > 0) SelectRank(ranks[0]);
    }

    public void SelectRank(int rank)
    {
        if (!rankRows.ContainsKey(rank)) return;
        if (SelectedRank == rank && SelectedMapData != null && stageManager != null &&
            stageManager.IsStageUnlocked(SelectedMapData)) return;

        ClearStages();
        SelectedRank = null;
        List<MapData> stages = stageManager == null
            ? new List<MapData>() : stageManager.GetAvailableStages(SelectedMapType, rank);
        if (stages.Count > 0) SelectedRank = rank;
        foreach (var entry in rankRows) entry.Value.Visual?.SetSelected(SelectedRank == entry.Key);

        foreach (MapData stage in stages)
        {
            Row row = CreateRow(stageButtonTemplate, stageContent,
                $"Stage_{stage.stageCode}", $"{stage.stageCode}  {stage.mapName}");
            if (row == null) continue;
            MapData capturedStage = stage;
            row.Button.onClick.AddListener(() => SelectStage(capturedStage));
            stageRows.Add(stage, row);
        }
        if (stages.Count > 0) SelectStage(stages[0]);
    }

    public void SelectStage(MapData stage)
    {
        if (stage == null || !stageRows.ContainsKey(stage) || stageManager == null ||
            !stageManager.IsStageUnlocked(stage) || stage.mapType != SelectedMapType || stage.rank != SelectedRank)
            return;
        foreach (var entry in stageRows) entry.Value.Visual?.SetSelected(entry.Key == stage);
        SetSelection(stage);
    }

    private void HideTemplates()
    {
        if (rankPlaceholders != null)
            foreach (GameObject placeholder in rankPlaceholders)
                if (placeholder != null) placeholder.SetActive(false);
        if (rankButtonTemplate != null) rankButtonTemplate.gameObject.SetActive(false);
        if (stageButtonTemplate != null) stageButtonTemplate.gameObject.SetActive(false);
    }

    private Row CreateRow(RectTransform template, RectTransform parent, string name, string text)
    {
        if (template == null || parent == null) return null;
        RectTransform root = Instantiate(template, parent, false);
        root.name = name;
        if (template == rankButtonTemplate)
        {
            LayoutElement layout = root.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.preferredWidth = template.rect.width;
                layout.flexibleWidth = 0;
            }
        }
        Button button = root.GetComponentInChildren<Button>(true);
        if (button == null)
        {
            Destroy(root.gameObject);
            return null;
        }

        // Only generated buttons lose template listeners; authored navigation buttons stay intact.
        button.onClick = new Button.ButtonClickedEvent();
        button.interactable = true;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = text;
        root.gameObject.SetActive(true);
        var visual = button.GetComponent<MenuButtonVisual>();
        visual?.SetSelected(false);
        return new Row { Root = root, Button = button, Visual = visual };
    }

    private void ClearStages()
    {
        ClearRows(stageRows.Values);
        stageRows.Clear();
        SetSelection(null);
    }

    private void ClearRows(IEnumerable<Row> rows)
    {
        foreach (Row row in rows)
        {
            if (row.Button != null) row.Button.onClick.RemoveAllListeners();
            if (row.Root == null) continue;
            // Destroy is deferred: deactivate first so old rows cannot receive another click.
            row.Root.gameObject.SetActive(false);
            Destroy(row.Root.gameObject);
        }
    }

    private void SetSelection(MapData stage)
    {
        bool changed = SelectedMapData != stage;
        SelectedMapData = stage;
        if (selectionButton != null) selectionButton.interactable = stage != null;
        if (stageNameText != null) stageNameText.text = stage != null ? stage.mapName : string.Empty;
        if (rankText != null) rankText.text = stage != null ? $"Rank : {stage.rank}" : string.Empty;
        if (corruptionText != null) corruptionText.text = stage != null ? $"침식도 : {stage.corruption}" : string.Empty;
        if (descriptionText != null) descriptionText.text = stage != null
            ? stage.description : string.Empty;
        if (thumbnail != null)
        {
            thumbnail.sprite = stage != null ? stage.thumbnail : null;
            thumbnail.preserveAspect = true;
            thumbnail.enabled = thumbnail.sprite != null;
        }
        detailsView?.ShowStage(stage);
        if (changed) SelectedStageChanged?.Invoke(stage);
    }
}
