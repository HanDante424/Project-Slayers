using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TownCharacterEquipmentController : MonoBehaviour
{
    [Serializable] public struct SlotView { public string key; public TMP_Text overviewName, mainName; public Image overviewIcon, mainIcon; public Sprite fallbackPartIcon; }
    [Serializable] public struct StatIcon { public StatType stat; public Sprite sprite; }
    [Serializable] public struct ItemView { public TMP_Text name, upgrade, stats, setName; public Image icon; public Button setButton; }
    [SerializeField] private IconRegistry iconRegistry;
    [SerializeField] private TownCharacterStatsController characterStats;
    [SerializeField] private TownCharacterDataSource dataSource;
    [SerializeField] private SlotView[] slots;
    [SerializeField] private Image summaryIcon;
    [SerializeField] private TMP_Text summaryName, summaryExtra;
    [SerializeField] private Transform setContent, setTitleTemplate, setDescriptionTemplate;
    [SerializeField] private Transform ownedContent, ownedTemplate;
    [SerializeField] private Transform equipmentDeltaContent, replacementDeltaContent, deltaTemplate;
    [SerializeField] private StatIcon[] statIcons;
    [SerializeField] private Sprite staggerGaugeIcon;
    [SerializeField] private ItemView selectedView, equippedView;
    [SerializeField] private Button equipButton, popupClose;
    [SerializeField] private GameObject setPopup;
    [SerializeField] private TMP_Text popupName, popupMembers, popupEffects, popupCount;
    [SerializeField] private GameObject[] authoredSamples;
    [SerializeField] private TMP_Text[] placeholderTexts;
    private readonly List<GameObject> setRows = new(), ownedRows = new(), deltaRows = new();
    private TownEquipmentItem selected;
    private PlayableCharacterData character;
    public string SelectedSlot { get; private set; }
    private PlayableCharacterData CurrentCharacter => characterStats != null ? characterStats.CurrentCharacter : TownCharacterSelection.Character;
    private CharacterStats CurrentStats => characterStats != null ? characterStats.CurrentStats : null;

    private void OnEnable()
    {
        TownCharacterSelection.Changed += Refresh;
        if (characterStats != null) characterStats.Refreshed += Refresh;
        if (dataSource != null) dataSource.Changed += Refresh;
        if (equipButton != null) equipButton.onClick.AddListener(EquipSelected);
        if (popupClose != null) popupClose.onClick.AddListener(CloseSetPopup);
        if (selectedView.setButton != null) selectedView.setButton.onClick.AddListener(OpenSelectedSet);
        if (equippedView.setButton != null) equippedView.setButton.onClick.AddListener(OpenEquippedSet);
        Refresh();
    }
    private void OnDisable()
    {
        TownCharacterSelection.Changed -= Refresh;
        if (characterStats != null) characterStats.Refreshed -= Refresh;
        if (dataSource != null) dataSource.Changed -= Refresh;
        if (equipButton != null) equipButton.onClick.RemoveListener(EquipSelected);
        if (popupClose != null) popupClose.onClick.RemoveListener(CloseSetPopup);
        if (selectedView.setButton != null) selectedView.setButton.onClick.RemoveListener(OpenSelectedSet);
        if (equippedView.setButton != null) equippedView.setButton.onClick.RemoveListener(OpenEquippedSet);
        Clear(setRows); Clear(ownedRows); Clear(deltaRows); CloseSetPopup();
    }
    public void Refresh()
    {
        if (!isActiveAndEnabled) return;
        if (authoredSamples != null) foreach (var sample in authoredSamples) if (sample != null) sample.SetActive(false);
        if (placeholderTexts != null) foreach (var text in placeholderTexts) Text(text, "");
        if (CurrentCharacter != character) { character = CurrentCharacter; selected = null; CloseSetPopup(); }
        Clear(setRows); Clear(ownedRows); Clear(deltaRows);
        foreach (var slot in slots ?? Array.Empty<SlotView>())
        {
            var item = dataSource != null ? dataSource.GetEquipped(character, slot.key) : null;
            bool valid = item?.Definition != null;
            Text(slot.overviewName, valid ? item.Definition.equipmentName : "없음"); Text(slot.mainName, valid ? item.Definition.equipmentName + (string.IsNullOrEmpty(item.UpgradeText) ? "" : " " + item.UpgradeText) : "없음");
            // These Images identify parts; actual item thumbnails belong to ItemView.
            var partIcon = (iconRegistry != null ? iconRegistry.GetEquipmentPartIcon(slot.key) : null) ?? slot.fallbackPartIcon;
            Icon(slot.overviewIcon, partIcon); Icon(slot.mainIcon, partIcon);
        }
        var sets = dataSource != null ? dataSource.GetSets(character) : null;
        var active = sets?.Where(s => s != null && s.Active).ToList() ?? new List<TownEquipmentSet>();
        Icon(summaryIcon, active.Count > 0 ? SetIcon(active[0]) : null); Text(summaryName, active.Count > 0 ? active[0].Name : ""); Text(summaryExtra, active.Count > 1 ? "+" + (active.Count - 1) : "");
        foreach (var set in active) AddSet(set);
        ShowDeltas(equipmentDeltaContent, CurrentStats, dataSource != null ? dataSource.GetStatsWithoutEquipment(character) : null);
        if (!string.IsNullOrEmpty(SelectedSlot)) BuildOwned();
        ShowComparison();
    }
    public void ShowSlot(string slot)
    {
        if (!(slots ?? Array.Empty<SlotView>()).Any(s => s.key == slot)) return;
        SelectedSlot = slot; selected = null; CloseSetPopup(); Refresh();
    }
    private void BuildOwned()
    {
        var owned = dataSource != null ? dataSource.GetOwned(character, SelectedSlot) : null;
        if (selected != null && !(owned?.Contains(selected) ?? false)) selected = null;
        foreach (var item in owned ?? Array.Empty<TownEquipmentItem>())
        {
            if (item?.Definition == null) continue;
            var root = Clone(ownedTemplate, ownedContent, ownedRows); if (root == null) continue;
            var button = root.GetComponentInChildren<Button>(true);
            Text(button?.GetComponentInChildren<TMP_Text>(true), item.Definition.equipmentName);
            Icon(root.transform.Find("Image")?.GetComponent<Image>(), item.Definition.thumbnail);
            if (button != null) { button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(() => SelectOwned(item)); }
        }
        if (ownedRows.Count == 0)
        {
            var root = Clone(ownedTemplate, ownedContent, ownedRows);
            var button = root != null ? root.GetComponentInChildren<Button>(true) : null;
            if (button != null) { button.onClick = new Button.ButtonClickedEvent(); button.interactable = false; Text(button.GetComponentInChildren<TMP_Text>(true), "없음"); }
            if (root != null) Icon(root.transform.Find("Image")?.GetComponent<Image>(), null);
        }
    }
    private void SelectOwned(TownEquipmentItem item)
    {
        var owned = dataSource != null ? dataSource.GetOwned(character, SelectedSlot) : null;
        if (item?.Definition == null || !(owned?.Contains(item) ?? false)) return;
        selected = item; Clear(deltaRows); ShowDeltas(equipmentDeltaContent, CurrentStats, dataSource.GetStatsWithoutEquipment(character)); ShowComparison();
    }
    private void ShowComparison()
    {
        var equipped = dataSource != null && SelectedSlot != null ? dataSource.GetEquipped(character, SelectedSlot) : null;
        ShowItem(selectedView, selected); ShowItem(equippedView, equipped);
        if (equipButton != null) equipButton.interactable = selected?.Definition != null && dataSource != null && dataSource.CanEquip(character, SelectedSlot, selected);
        // The evaluator owns replacement semantics and set thresholds; this view only subtracts final snapshots.
        var after = selected != null && dataSource != null ? dataSource.GetReplacementStats(character, SelectedSlot, selected) : null;
        ShowDeltas(replacementDeltaContent, after, CurrentStats);
    }
    private void ShowItem(ItemView view, TownEquipmentItem item)
    {
        var definition = item?.Definition;
        Text(view.name, definition != null ? definition.equipmentName : "없음"); Text(view.upgrade, definition != null ? item.UpgradeText : ""); Icon(view.icon, definition != null ? definition.thumbnail : null);
        Text(view.stats, definition != null ? FormatEquipment(definition) : "");
        var set = FindSet(definition?.set); Text(view.setName, set?.Name ?? "없음");
        if (view.setButton != null) view.setButton.interactable = set != null;
    }
    private static string FormatEquipment(EquipmentData item)
    {
        var entries = new[] { item.baseStatModifiers, item.detailStatModifiers, item.elementEnhancementModifiers, item.elementResistanceModifiers };
        return string.Join("\n", entries.Where(e => e != null).SelectMany(e => e).Where(e => e != null).Select(e => StatName(e.statType) + " " + (e.value > 0 ? "+" : "") + CharacterStatPanelView.FormatNumber(e.value) + (e.modifierType == StatModifierType.Percent ? "%" : "")));
    }
    public static IEnumerable<(StatType stat, float delta)> GetDifferences(CharacterStats after, CharacterStats before)
    {
        if (after == null || before == null || after.Data == null || before.Data != after.Data || before.Level != after.Level) yield break;
        foreach (StatType stat in Enum.GetValues(typeof(StatType))) { float delta = after.GetFinalStat(stat) - before.GetFinalStat(stat); if (delta != 0) yield return (stat, delta); }
    }
    private void ShowDeltas(Transform parent, CharacterStats after, CharacterStats before)
    {
        if (parent == null) return;
        var values = GetDifferences(after, before).Select(e => (name: StatName(e.stat), value: (e.delta > 0 ? "+" : "") + CharacterStatPanelView.FormatStat(e.stat, e.delta), sprite: (iconRegistry != null ? iconRegistry.GetStatIcon(e.stat) : null) ?? (statIcons ?? Array.Empty<StatIcon>()).FirstOrDefault(i => i.stat == e.stat).sprite)).ToList();
        if (after != null && before != null && after.Data != null && after.Data == before.Data && after.Level == before.Level)
        {
            float gauge = after.FinalStaggerGauge - before.FinalStaggerGauge;
            if (gauge != 0) values.Add(("무력화 게이지", (gauge > 0 ? "+" : "") + CharacterStatPanelView.FormatNumber(gauge), (iconRegistry != null ? iconRegistry.GetStaggerGaugeIcon() : null) ?? staggerGaugeIcon));
        }
        for (int i = 0; i < values.Count; i += 2)
        {
            var root = Clone(deltaTemplate, parent, deltaRows); if (root == null) continue;
            int column = 0;
            foreach (Transform cell in root.transform)
            {
                int index = i + column++; bool valid = index < values.Count;
                Text(cell.GetComponentInChildren<TMP_Text>(true), valid ? values[index].name + " " + values[index].value : "");
                Icon(cell.Find("Stat")?.GetComponent<Image>(), valid ? values[index].sprite : null);
            }
        }
    }
    private static string StatName(StatType stat) => stat switch
    {
        StatType.Health => "체력", StatType.Mana => "마나", StatType.Attack => "공격력", StatType.Magic => "마력", StatType.Defense => "방어력", StatType.Resistance => "저항력", StatType.DefensePenetration => "방어 관통", StatType.ResistancePenetration => "저항 관통",
        StatType.Stagger => "무력화 증가", StatType.CriticalChance => "치명타 확률", StatType.CriticalDamage => "치명타 피해", StatType.AttackSpeed => "공격속도", StatType.MoveSpeed => "이동속도", StatType.Accuracy => "적중률", StatType.Evasion => "회피율", StatType.StatusResistance => "상태이상 저항력", StatType.Recovery => "회복력", StatType.CooldownReduction => "쿨타임 감소", StatType.Luck => "행운",
        StatType.FireEnhancement => "화염 강화", StatType.WaterEnhancement => "물 강화", StatType.EarthEnhancement => "대지 강화", StatType.WindEnhancement => "바람 강화", StatType.LightningEnhancement => "번개 강화", StatType.IceEnhancement => "얼음 강화", StatType.LightEnhancement => "빛 강화", StatType.DarkEnhancement => "어둠 강화",
        StatType.FireResistance => "화염 저항", StatType.WaterResistance => "물 저항", StatType.EarthResistance => "대지 저항", StatType.WindResistance => "바람 저항", StatType.LightningResistance => "번개 저항", StatType.IceResistance => "얼음 저항", StatType.LightResistance => "빛 저항", StatType.DarkResistance => "어둠 저항", _ => stat.ToString()
    };
    private Sprite SetIcon(TownEquipmentSet set) => (iconRegistry != null ? iconRegistry.GetSetIcon(set.Definition) : null) ?? set.Icon;
    private void AddSet(TownEquipmentSet set)
    {
        var title = Clone(setTitleTemplate, setContent, setRows); var desc = Clone(setDescriptionTemplate, setContent, setRows);
        if (title == null || desc == null) return;
        var button = title.GetComponentInChildren<Button>(true); Text(button?.GetComponentInChildren<TMP_Text>(true), set.Name); Icon(title.transform.Find("Image")?.GetComponent<Image>(), SetIcon(set));
        var texts = desc.GetComponentsInChildren<TMP_Text>(true); for (int i = 0; i < texts.Length; i++) { Text(texts[i], ""); texts[i].gameObject.SetActive(i == 0); } if (texts.Length > 0) Text(texts[0], set.Effects);
        desc.SetActive(false);
        if (button != null) { button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(() => ToggleSetDescription(desc, (RectTransform)setContent)); }
    }
    public static void ToggleSetDescription(GameObject description, RectTransform content)
    {
        if (description == null) return;
        description.SetActive(!description.activeSelf);
        if (content != null) LayoutRebuilder.MarkLayoutForRebuild(content);
    }
    private TownEquipmentSet FindSet(EquipmentSetData definition) => definition == null || dataSource == null ? null : dataSource.GetSets(character)?.FirstOrDefault(s => s != null && s.Definition == definition);
    public void OpenSetPopup(TownEquipmentSet set)
    {
        if (set?.Definition == null || dataSource == null) return;
        set = FindSet(set.Definition);
        if (set == null) return;
        Text(popupName, set.Name); Text(popupMembers, set.Members); Text(popupEffects, set.Effects); Text(popupCount, "장착 수량 : " + set.EquippedCount);
        if (setPopup != null) setPopup.SetActive(true);
    }
    private void OpenSelectedSet() => OpenSetPopup(FindSet(selected?.Definition?.set));
    private void OpenEquippedSet() => OpenSetPopup(FindSet(dataSource != null ? dataSource.GetEquipped(character, SelectedSlot)?.Definition?.set : null));
    public void CloseSetPopup() { if (setPopup != null) setPopup.SetActive(false); }
    private void EquipSelected()
    {
        if (selected?.Definition == null || dataSource == null || !(dataSource.GetOwned(character, SelectedSlot)?.Contains(selected) ?? false) || !dataSource.CanEquip(character, SelectedSlot, selected)) return;
        if (dataSource.TryEquip(character, SelectedSlot, selected)) { selected = null; characterStats?.RefreshCharacter(); Refresh(); }
    }
    private GameObject Clone(Transform template, Transform parent, List<GameObject> list)
    {
        if (template == null || parent == null) return null;
        var root = Instantiate(template.gameObject, parent, false); list.Add(root); root.SetActive(true); return root;
    }
    private void Clear(List<GameObject> rows)
    {
        foreach (var row in rows) { if (row == null) continue; foreach (var button in row.GetComponentsInChildren<Button>(true)) button.onClick.RemoveAllListeners(); row.SetActive(false); Destroy(row); } rows.Clear();
    }
    private static void Text(TMP_Text text, string value) { if (text != null) text.text = value; }
    private static void Icon(Image image, Sprite sprite) { if (image != null) { image.sprite = sprite; image.enabled = sprite != null; } }
}
