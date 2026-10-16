using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TownStageDetailsView : MonoBehaviour
{
    [Header("Existing Enemy List")]
    [SerializeField] private RectTransform enemyContent;
    [SerializeField] private RectTransform enemyTemplate;
    [SerializeField] private TMP_Text enemyNameText;
    [Header("Existing Drop List and Detail")]
    [SerializeField] private RectTransform dropContent;
    [SerializeField] private RectTransform dropTemplate;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text dropCountText;
    [SerializeField] private TMP_Text dropChanceText;
    [SerializeField] private TMP_Text dropSourceText;
    [Header("Unconnected Sample Content")]
    [SerializeField] private TMP_Text[] sampleTexts;
    [SerializeField] private Image[] sampleImages;

    private MapData selectedStage;
    private sealed class DetailRow
    {
        public RectTransform Root;
        public Button Button;
        public MenuButtonVisual Visual;
        public MonsterData Monster;
        public MonsterItemDrop Drop;
    }
    private readonly List<DetailRow> enemyRows = new();
    private readonly List<DetailRow> dropRows = new();

    public void ShowStage(MapData stage)
    {
        selectedStage = stage;
        if (!isActiveAndEnabled)
        {
            ClearRows(enemyRows);
            ClearRows(dropRows);
            ClearDetails();
            return;
        }
        ClearSamples();
        RefreshEnemies();
        RefreshRewards();
    }

    public void RefreshEnemies()
    {
        if (!isActiveAndEnabled) return;
        ClearRows(enemyRows);
        if (enemyTemplate != null) enemyTemplate.gameObject.SetActive(false);
        SetText(enemyNameText, string.Empty);
        if (selectedStage != null && selectedStage.enemies != null)
        {
            foreach (MonsterData monster in selectedStage.enemies)
            {
                if (monster == null) continue;
                DetailRow row = CreateRow(enemyTemplate, enemyContent, "Enemy_" + enemyRows.Count, monster.monsterName);
                if (row == null) continue;
                row.Monster = monster;
                row.Button.onClick.AddListener(() => SelectEnemy(row));
                enemyRows.Add(row);
            }
        }
        if (enemyRows.Count > 0) SelectEnemy(enemyRows[0]);
        else
        {
            string message = selectedStage == null ? "선택된 스테이지 없음" : "등록된 몬스터 없음";
            AddEmptyRow(enemyTemplate, enemyContent, enemyRows, message);
            SetText(enemyNameText, message);
        }
    }

    public void RefreshRewards()
    {
        if (!isActiveAndEnabled) return;
        ClearRows(dropRows);
        if (dropTemplate != null) dropTemplate.gameObject.SetActive(false);
        ClearDropDetails();
        if (selectedStage != null && selectedStage.enemies != null)
        {
            foreach (MonsterData monster in selectedStage.enemies)
            {
                if (monster == null || monster.itemDrops == null) continue;
                foreach (MonsterItemDrop drop in monster.itemDrops)
                {
                    if (drop == null || drop.item == null) continue;
                    DetailRow row = CreateRow(dropTemplate, dropContent, "Drop_" + dropRows.Count, drop.item.itemName);
                    if (row == null) continue;
                    // Keep each monster/drop pair separate, even when ItemData is shared.
                    row.Monster = monster;
                    row.Drop = drop;
                    row.Button.onClick.AddListener(() => SelectDrop(row));
                    dropRows.Add(row);
                }
            }
        }
        if (dropRows.Count > 0) SelectDrop(dropRows[0]);
        else
        {
            string message = selectedStage == null ? "선택된 스테이지 없음" : "등록된 드롭 아이템 없음";
            AddEmptyRow(dropTemplate, dropContent, dropRows, message);
            SetText(itemNameText, message);
        }
    }

    private void SelectEnemy(DetailRow selected)
    {
        if (!enemyRows.Contains(selected) || selected.Monster == null) return;
        foreach (DetailRow row in enemyRows) row.Visual?.SetSelected(row == selected);
        SetText(enemyNameText, selected.Monster.monsterName);
    }

    private void SelectDrop(DetailRow selected)
    {
        if (!dropRows.Contains(selected) || selected.Monster == null || selected.Drop == null || selected.Drop.item == null)
        {
            ClearDropDetails();
            return;
        }
        foreach (DetailRow row in dropRows) row.Visual?.SetSelected(row == selected);
        SetText(itemNameText, selected.Drop.item.itemName);
        SetText(dropCountText, "1개");
        SetText(dropChanceText, selected.Drop.dropChance.ToString("G", CultureInfo.InvariantCulture) + "%");
        SetText(dropSourceText, "드롭 몬스터 : " + selected.Monster.monsterName);
    }

    private DetailRow CreateRow(RectTransform template, RectTransform parent, string rowName, string text)
    {
        if (template == null || parent == null) return null;
        RectTransform root = Instantiate(template, parent, false);
        root.name = rowName;
        Button button = root.GetComponentInChildren<Button>(true);
        if (button == null)
        {
            root.gameObject.SetActive(false);
            Destroy(root.gameObject);
            return null;
        }
        button.onClick = new Button.ButtonClickedEvent();
        button.interactable = true;
        SetText(button.GetComponentInChildren<TMP_Text>(true), text);
        root.gameObject.SetActive(true);
        MenuButtonVisual visual = button.GetComponent<MenuButtonVisual>();
        visual?.SetSelected(false);
        return new DetailRow { Root = root, Button = button, Visual = visual };
    }

    private void AddEmptyRow(RectTransform template, RectTransform content, List<DetailRow> rows, string text)
    {
        DetailRow row = CreateRow(template, content, "Empty", text);
        if (row == null) return;
        row.Button.interactable = false;
        rows.Add(row);
    }

    private void ClearRows(List<DetailRow> rows)
    {
        foreach (DetailRow row in rows)
        {
            if (row.Button != null) row.Button.onClick.RemoveAllListeners();
            if (row.Root == null) continue;
            row.Root.gameObject.SetActive(false);
            Destroy(row.Root.gameObject);
        }
        rows.Clear();
    }

    private void ClearSamples()
    {
        if (sampleTexts != null) foreach (TMP_Text text in sampleTexts) SetText(text, string.Empty);
        if (sampleImages != null) foreach (Image image in sampleImages) if (image != null) image.enabled = false;
    }

    private void ClearDropDetails()
    {
        SetText(itemNameText, string.Empty);
        SetText(dropCountText, string.Empty);
        SetText(dropChanceText, string.Empty);
        SetText(dropSourceText, string.Empty);
    }

    private void ClearDetails()
    {
        SetText(enemyNameText, string.Empty);
        ClearDropDetails();
    }

    private void OnDisable()
    {
        ClearRows(enemyRows);
        ClearRows(dropRows);
        ClearDetails();
        selectedStage = null;
    }

    private static void SetText(TMP_Text target, string text)
    {
        if (target != null) target.text = text;
    }
}
