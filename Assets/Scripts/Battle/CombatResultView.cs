using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CombatResultView : MonoBehaviour
{
    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text stageDescription;
    [SerializeField] private TMP_Text stageName;
    [SerializeField] private TMP_Text clearTime;
    [SerializeField] private TMP_Text kills;
    [SerializeField] private TMP_Text damageDealt;
    [SerializeField] private TMP_Text damageReceived;
    [SerializeField] private TMP_Text rewardTitle;
    [SerializeField] private TMP_Text gold;
    [SerializeField] private TMP_Text memoryFragment;
    [SerializeField] private ScrollRect itemScroll;
    [SerializeField] private CombatResultItemView itemTemplate;
    [SerializeField] private GameObject[] sampleItems;
    [SerializeField] private Transform retryButtonRoot;
    [SerializeField] private Transform townButtonRoot;
    private readonly List<CombatResultItemView> rows = new();
    public Button RetryButton => retryButtonRoot.GetComponentInChildren<Button>(true);
    public Button TownButton => townButtonRoot.GetComponentInChildren<Button>(true);
    public bool IsVisible => screen.activeSelf;

    public void ResetView()
    {
        screen.SetActive(false);
        ClearItems();
        foreach (var sample in sampleItems) if (sample != null) sample.SetActive(false);
        itemTemplate.gameObject.SetActive(false);
        foreach (var text in new[] { title, stageDescription, stageName, clearTime, kills,
            damageDealt, damageReceived, rewardTitle, gold, memoryFragment }) text.text = "";
        SetButtonsEnabled(true);
    }

    public void Show(CombatResultData result)
    {
        title.text = result.IsTestClear ? "TEST CLEAR" : "STAGE CLEAR";
        stageDescription.text = result.Stage != null
            ? $"{MapTypeName(result.Stage.mapType)} - {result.Stage.rank}등급 - 침식도 {result.Stage.corruption}"
            : "Dev_Combat 단독 테스트 · 스테이지 미선택";
        stageName.text = result.Stage != null ? result.Stage.mapName : "스테이지 정보 없음";
        clearTime.text = CombatRecord.FormatTime(result.ElapsedSeconds);
        kills.text = result.Kills.ToString();
        damageDealt.text = result.DamageDealt.ToString();
        damageReceived.text = result.DamageReceived.ToString();
        rewardTitle.text = "획득 보상 · 지급 미구현";
        gold.text = "골드 · 미지급";
        memoryFragment.text = "기억 파편 · 미지급";
        ClearItems(); // No actual granted items exist yet; do not display sample drops as rewards.
        screen.SetActive(true);
        Canvas.ForceUpdateCanvases();
        itemScroll.StopMovement();
        itemScroll.verticalNormalizedPosition = 1f;
    }

    // Presentation only: future callers must pass receipts, not drop definitions or previews.
    public void AddGrantedItem(ItemData item, int count)
    {
        if (item == null || count <= 0) return;
        var row = Instantiate(itemTemplate, itemScroll.content, false);
        row.Bind(item, count);
        row.gameObject.SetActive(true);
        rows.Add(row);
    }

    private void ClearItems()
    {
        foreach (var row in rows)
            if (row != null) { row.gameObject.SetActive(false); Destroy(row.gameObject); }
        rows.Clear();
    }

    public void SetButtonsEnabled(bool enabled)
    {
        RetryButton.interactable = enabled;
        TownButton.interactable = enabled;
    }

    private static string MapTypeName(MapType type) => type switch
    {
        MapType.Search => "수색전", MapType.Defense => "수성전", MapType.Attack => "공격전",
        MapType.Conquest => "공략전", MapType.Raid => "레이드", _ => type.ToString()
    };
}
