using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TownBlacksmithCraftController : MonoBehaviour
{
    [SerializeField] private ItemEquipmentRegistry registry;
    [SerializeField] private IconRegistry iconRegistry;
    [SerializeField] private TownCraftingDataSource dataSource;
    [Header("Existing list and templates")]
    [SerializeField] private Transform itemContent;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private Transform statContent;
    [SerializeField] private Transform statTemplate;
    [SerializeField] private Transform materialContent;
    [SerializeField] private Transform materialTemplate;
    [SerializeField] private GameObject[] authoredSamples;
    [Header("Existing equipment information")]
    [SerializeField] private Image thumbnail;
    [SerializeField] private Image setIcon;
    [SerializeField] private Sprite fallbackSetIcon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text partText;
    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private TMP_Text setNameText;
    [SerializeField] private TMP_Text[] setThresholdTexts;
    [Header("Existing crafting controls")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text memoryText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button setButton;
    [SerializeField] private Button popupCloseButton;
    [Header("Existing set popup")]
    [SerializeField] private GameObject setPopup;
    [SerializeField] private TMP_Text popupName;
    [SerializeField] private Transform memberContent;
    [SerializeField] private Transform memberTemplate;
    [SerializeField] private Transform effectContent;
    [SerializeField] private Transform effectTemplate;
    private readonly List<GameObject> itemRows = new(), statRows = new(), materialRows = new(), memberRows = new(), effectRows = new();
    private readonly List<EquipmentData> available = new();
    public EquipmentSlot SelectedSlot { get; private set; } = EquipmentSlot.Unassigned;
    public EquipmentData SelectedEquipment { get; private set; }
    public int Quantity { get; private set; } = 1;
    public bool IsCraftMode { get; private set; } = true;

    private void OnEnable()
    {
        // The authored full-panel frames sit above the quantity controls; they are decoration only.
        foreach (var image in GetComponentsInChildren<Image>(true))
            if (image.name == "Frame" || image.name == "BackGround") image.raycastTarget = false;
        PreserveArrowVisual(increaseButton); PreserveArrowVisual(decreaseButton);
        Bind(increaseButton, Increase); Bind(decreaseButton, Decrease); Bind(resetButton, ResetQuantity);
        Bind(craftButton, Craft); Bind(setButton, OpenSetPopup); Bind(popupCloseButton, CloseSetPopup);
        if (dataSource != null) dataSource.Changed += RefreshRecipe;
        // This authored template action belongs to the asset's sample menu, not the equipment set popup.
        if (setButton != null)
            for (int i = 0; i < setButton.onClick.GetPersistentEventCount(); i++)
                if (setButton.onClick.GetPersistentMethodName(i) == "NewGame")
                    setButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
        Refresh();
    }
    private void OnDisable()
    {
        Unbind(increaseButton, Increase); Unbind(decreaseButton, Decrease); Unbind(resetButton, ResetQuantity);
        Unbind(craftButton, Craft); Unbind(setButton, OpenSetPopup); Unbind(popupCloseButton, CloseSetPopup);
        if (dataSource != null) dataSource.Changed -= RefreshRecipe;
        foreach (var rows in new[] { itemRows, statRows, materialRows, memberRows, effectRows }) Clear(rows);
        available.Clear(); SelectedEquipment = null; CloseSetPopup();
    }
    public void ShowSlot(EquipmentSlot slot, bool craftMode)
    {
        SelectedSlot = slot; IsCraftMode = craftMode; SelectedEquipment = null; Quantity = 1;
        if (isActiveAndEnabled) Refresh();
    }
    public void Refresh()
    {
        if (!isActiveAndEnabled) return;
        foreach (var sample in authoredSamples ?? Array.Empty<GameObject>()) if (sample != null) sample.SetActive(false);
        CloseSetPopup(); available.Clear();
        if (IsCraftMode && registry != null)
            available.AddRange(registry.GetAllEquipment().Where(e => e.slot == SelectedSlot));
        if (!available.Contains(SelectedEquipment)) { SelectedEquipment = available.FirstOrDefault(); Quantity = 1; }
        int count = Math.Max(1, available.Count);
        for (int i = 0; i < count; i++)
        {
            var row = Row(itemRows, itemTemplate, itemContent, i); if (row == null) continue;
            var definition = available.Count > 0 ? available[i] : null;
            var button = row.GetComponentInChildren<Button>(true);
            Text(button?.GetComponentInChildren<TMP_Text>(true), definition != null ? definition.equipmentName : (IsCraftMode ? "등록된 장비 없음" : "강화 장비 정보 없음"));
            Icon(row.transform.Find("Image")?.GetComponent<Image>(), definition != null ? definition.thumbnail : null);
            if (button != null)
            {
                button.onClick = new Button.ButtonClickedEvent(); button.interactable = definition != null;
                if (definition != null) button.onClick.AddListener(() => SelectEquipment(definition));
                TownButtonBinder.GetVisual(button).SetSelected(definition != null && definition == SelectedEquipment);
            }
            row.SetActive(true);
        }
        HideExtra(itemRows, count); RefreshDetails();
    }
    public void SelectEquipment(EquipmentData equipment)
    {
        if (equipment == null || !available.Contains(equipment)) return;
        SelectedEquipment = equipment; Quantity = 1; CloseSetPopup();
        for (int i = 0; i < available.Count && i < itemRows.Count; i++)
        {
            var button = itemRows[i].GetComponentInChildren<Button>(true);
            if (button != null) TownButtonBinder.GetVisual(button).SetSelected(available[i] == equipment);
        }
        RefreshDetails();
    }
    private void RefreshDetails()
    {
        var definition = SelectedEquipment;
        Text(nameText, definition != null ? definition.equipmentName : "없음"); Text(upgradeText, "");
        Text(rankText, definition != null ? definition.rank + "등급" : "");
        Text(partText, definition != null ? PartName(definition.slot) : "");
        Icon(thumbnail, definition != null ? definition.thumbnail : null);
        var modifiers = definition != null ? new[] { definition.baseStatModifiers, definition.detailStatModifiers, definition.elementEnhancementModifiers, definition.elementResistanceModifiers }.Where(l => l != null).SelectMany(l => l).Where(m => m != null).ToList() : new List<EquipmentStatModifier>();
        for (int i = 0; i < modifiers.Count; i++)
        {
            var row = Row(statRows, statTemplate, statContent, i); if (row == null) continue;
            Text(row.transform.Find("Text")?.GetComponent<TMP_Text>(), Describe(modifiers[i]));
            Icon(row.transform.Find("Stat")?.GetComponent<Image>(), iconRegistry != null ? iconRegistry.GetStatIcon(modifiers[i].statType) : null);
            row.SetActive(true);
        }
        HideExtra(statRows, modifiers.Count);
        var set = definition != null ? definition.set : null;
        Text(setNameText, set != null ? set.setName : "없음");
        Icon(setIcon, set != null ? ((iconRegistry != null ? iconRegistry.GetSetIcon(set) : null) ?? fallbackSetIcon) : null);
        if (setButton != null) setButton.interactable = set != null;
        for (int i = 0; i < (setThresholdTexts?.Length ?? 0); i++)
            Text(setThresholdTexts[i], set != null && (set.bonuses?.Any(b => b != null && b.requiredEquipmentCount == i + 1) ?? false) ? (i + 1) + "세트" : "");
        RefreshRecipe();
    }
    public void RefreshRecipe()
    {
        if (!isActiveAndEnabled) return;
        var definition = SelectedEquipment;
        Text(quantityText, definition != null ? Quantity.ToString(CultureInfo.InvariantCulture) : "");
        Text(goldText, definition != null ? Number((long)definition.craftingGold * Quantity) : "");
        Text(memoryText, definition != null ? Number((long)definition.craftingMemoryFragments * Quantity) : "");
        var materials = definition?.craftingMaterials?.Where(m => m != null && m.material != null).ToList() ?? new List<EquipmentCraftingMaterial>();
        for (int i = 0; i < materials.Count; i++)
        {
            var row = Row(materialRows, materialTemplate, materialContent, i); if (row == null) continue;
            var entry = materials[i]; long required = (long)entry.quantity * Quantity;
            Text(row.transform.Find("ItemName")?.GetComponent<TMP_Text>(), entry.material.itemName);
            Text(row.transform.Find("ItemCount")?.GetComponent<TMP_Text>(), dataSource != null ? Number(dataSource.GetOwnedMaterialCount(entry.material)) + " / " + Number(required) : "필요 : " + Number(required) + "개");
            Icon(row.transform.Find("Image")?.GetComponent<Image>(), entry.material.thumbnail); row.SetActive(true);
        }
        HideExtra(materialRows, materials.Count);
        if (increaseButton != null) increaseButton.interactable = definition != null && Quantity < int.MaxValue;
        if (decreaseButton != null) decreaseButton.interactable = definition != null && Quantity > 1;
        if (resetButton != null) resetButton.interactable = definition != null;
        if (craftButton != null) craftButton.interactable = IsCraftMode && definition != null && dataSource != null && dataSource.CanCraft(definition, Quantity);
    }
    public void Increase() { if (SelectedEquipment == null || Quantity == int.MaxValue) return; Quantity++; RefreshRecipe(); }
    public void Decrease() { if (Quantity <= 1) return; Quantity--; RefreshRecipe(); }
    public void ResetQuantity() { Quantity = 1; RefreshRecipe(); }
    private void Craft()
    {
        if (!IsCraftMode || SelectedEquipment == null || dataSource == null || !dataSource.CanCraft(SelectedEquipment, Quantity)) return;
        if (dataSource.TryCraft(SelectedEquipment, Quantity)) RefreshRecipe();
    }
    public void OpenSetPopup()
    {
        var set = SelectedEquipment != null ? SelectedEquipment.set : null;
        if (set == null || setPopup == null) return;
        Text(popupName, set.setName);
        var members = registry != null ? registry.GetAllEquipment().Where(e => e.set == set).ToList() : new List<EquipmentData>();
        for (int i = 0; i < members.Count; i++)
        {
            var row = Row(memberRows, memberTemplate, memberContent, i); if (row == null) continue;
            Text(row.transform.Find("Title")?.GetComponent<TMP_Text>(), members[i].equipmentName);
            Icon(row.transform.Find("Image")?.GetComponent<Image>(), members[i].thumbnail); row.SetActive(true);
        }
        HideExtra(memberRows, members.Count);
        var bonuses = set.bonuses?.Where(b => b != null).OrderBy(b => b.requiredEquipmentCount).ToList() ?? new List<EquipmentSetBonus>();
        for (int i = 0; i < bonuses.Count; i++)
        {
            var row = Row(effectRows, effectTemplate, effectContent, i); if (row == null) continue;
            Text(row.GetComponent<TMP_Text>(), bonuses[i].requiredEquipmentCount + "세트\n" + string.Join("\n", (bonuses[i].statModifiers ?? new List<EquipmentStatModifier>()).Where(m => m != null).Select(Describe)));
            row.SetActive(true);
        }
        HideExtra(effectRows, bonuses.Count); setPopup.SetActive(true);
    }
    public void CloseSetPopup() { if (setPopup != null) setPopup.SetActive(false); }
    private static GameObject Row(List<GameObject> pool, Transform template, Transform content, int index)
    {
        if (template == null || content == null) return null;
        while (pool.Count <= index)
        {
            var row = Instantiate(template.gameObject, content, false); row.SetActive(false); pool.Add(row);
            var line = content.Find("Line"); if (line != null) row.transform.SetSiblingIndex(line.GetSiblingIndex());
        }
        return pool[index];
    }
    private static void HideExtra(List<GameObject> rows, int count) { for (int i = count; i < rows.Count; i++) rows[i].SetActive(false); }
    private static void Clear(List<GameObject> rows) { foreach (var row in rows) if (row != null) { row.SetActive(false); Destroy(row); } rows.Clear(); }
    private static void Text(TMP_Text target, string value) { if (target != null) target.text = value; }
    private static void Icon(Image target, Sprite sprite)
    {
        if (target == null) return;
        // These authored icon slots must not expand to a new SO sprite's native pixel size.
        var fitter = target.GetComponent<ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
        target.sprite = sprite; target.preserveAspect = true; target.enabled = sprite != null;
    }
    private static string Number(long value) => value.ToString("N0", CultureInfo.InvariantCulture);
    private static string Describe(EquipmentStatModifier modifier) => TownCharacterEquipmentController.StatName(modifier.statType) + " " + (modifier.value > 0 ? "+" : "") + CharacterStatPanelView.FormatNumber(modifier.value) + (modifier.modifierType == StatModifierType.Percent ? "%" : "");
    private static string PartName(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Belt => "벨트", EquipmentSlot.Bracelet => "팔찌", EquipmentSlot.Necklace => "목걸이", EquipmentSlot.Gloves => "장갑", EquipmentSlot.HairAccessory => "머리 장식", EquipmentSlot.BackAccessory => "등 장식", EquipmentSlot.Earrings => "귀걸이", EquipmentSlot.Rings => "반지", EquipmentSlot.Badge => "뱃지", _ => ""
    };
    private static void Bind(Button button, UnityEngine.Events.UnityAction action) { if (button != null) button.onClick.AddListener(action); }
    private static void Unbind(Button button, UnityEngine.Events.UnityAction action) { if (button != null) button.onClick.RemoveListener(action); }
    private static void PreserveArrowVisual(Button button)
    {
        if (button == null) return;
        // Arrow sprites have their own authored states; a menu-background tint turns them into black squares.
        var visual = button.GetComponent<MenuButtonVisual>(); if (visual != null) visual.enabled = false;
        // The imported arrow buttons inherit rectangular menu Sprite Swap states.
        // Tint their original arrow sprite instead, including hover and disabled states.
        button.transition = Selectable.Transition.ColorTint;
        if (button.targetGraphic is Image image) image.overrideSprite = null;
        if (button.targetGraphic != null) button.targetGraphic.color = button.colors.normalColor;
    }
}
