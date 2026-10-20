using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Development-only inventory tools. No prices or production quest rules are inferred here.
public sealed class TownDevShopController : MonoBehaviour
{
    public enum Mode { Buy, Sell, Quest }
    public enum Currency { None, Gold, MemoryFragments }
    private const long CurrencyPackSize = 10000;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite memoryFragmentIcon;
    [SerializeField] private ItemEquipmentRegistry registry;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private Transform[] authoredSamples;
    [SerializeField] private Image thumbnail;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text ownedText;
    [SerializeField] private GameObject exchangeControls;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Transform resetButtonRoot;
    [SerializeField] private Transform exchangeButtonRoot;
    private Button resetButton, exchangeButton;
    private SharedInventory inventory;
    private readonly List<GameObject> rows = new();
    private readonly List<ItemData> available = new();
    public Mode CurrentMode { get; private set; }
    public ItemData SelectedItem { get; private set; }
    public Currency SelectedCurrency { get; private set; }
    public int Quantity { get; private set; } = 1;
    public int VisibleCount => available.Count + (CurrentMode == Mode.Buy ? 2 : 0);

    private void OnEnable()
    {
        inventory = InventorySession.Inventory;
        inventory.Changed += Refresh;
        resetButton = TownButtonBinder.Bind(resetButtonRoot, ResetQuantity, this);
        exchangeButton = TownButtonBinder.Bind(exchangeButtonRoot, Exchange, this);
        if (increaseButton != null) increaseButton.onClick.AddListener(Increase);
        if (decreaseButton != null) decreaseButton.onClick.AddListener(Decrease);
        // The user's decorative frames must not intercept the controls underneath them.
        foreach (var image in GetComponentsInChildren<Image>(true))
            if (image.name == "Frame") image.raycastTarget = false;
        Refresh();
    }
    private void OnDisable()
    {
        if (inventory != null) inventory.Changed -= Refresh;
        TownButtonBinder.Unbind(resetButton, ResetQuantity);
        TownButtonBinder.Unbind(exchangeButton, Exchange);
        if (increaseButton != null) increaseButton.onClick.RemoveListener(Increase);
        if (decreaseButton != null) decreaseButton.onClick.RemoveListener(Decrease);
        foreach (var row in rows) if (row != null) { row.SetActive(false); Destroy(row); }
        rows.Clear(); available.Clear(); SelectedItem = null; SelectedCurrency = Currency.None;
    }
    public void Show(Mode mode)
    {
        CurrentMode = mode; SelectedItem = null; SelectedCurrency = Currency.None; Quantity = 1;
        if (isActiveAndEnabled) Refresh();
    }
    public void Refresh()
    {
        if (!isActiveAndEnabled) return;
        foreach (var sample in authoredSamples ?? Array.Empty<Transform>()) if (sample != null) sample.gameObject.SetActive(false);
        available.Clear();
        if (CurrentMode == Mode.Buy && registry != null) available.AddRange(registry.GetAllItems());
        if (CurrentMode == Mode.Sell && inventory != null)
            foreach (var item in inventory.Items) available.Add(item.Definition);
        if (CurrentMode != Mode.Buy) SelectedCurrency = Currency.None;
        if (SelectedCurrency == Currency.None && !available.Contains(SelectedItem))
        {
            SelectedItem = available.Count > 0 ? available[0] : null; Quantity = 1;
            if (CurrentMode == Mode.Buy && SelectedItem == null) SelectedCurrency = Currency.Gold;
        }
        int count = CurrentMode == Mode.Buy ? available.Count : Math.Max(1, available.Count);
        for (int i = 0; i < count; i++)
        {
            var row = GetRow(i); if (row == null) continue;
            var definition = i < available.Count ? available[i] : null;
            var button = row.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = definition != null ? definition.itemName :
                    CurrentMode == Mode.Quest ? "퀘스트 준비 중" : "판매할 아이템 없음";
                // Only generated clones are rebound; authored scene events are never erased here.
                button.onClick = new Button.ButtonClickedEvent();
                button.interactable = definition != null;
                if (definition != null) button.onClick.AddListener(() => SelectItem(definition));
                TownButtonBinder.GetVisual(button).SetSelected(definition != null && definition == SelectedItem);
                var icon = button.transform.Find("Image")?.GetComponent<Image>();
                if (icon != null)
                {
                    var fitter = icon.GetComponent<ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
                    icon.sprite = definition != null ? definition.thumbnail : null;
                    icon.preserveAspect = true; icon.enabled = icon.sprite != null;
                }
            }
            row.SetActive(true);
        }
        if (CurrentMode == Mode.Buy)
        {
            ShowCurrencyRow(count++, Currency.Gold);
            ShowCurrencyRow(count++, Currency.MemoryFragments);
        }
        for (int i = count; i < rows.Count; i++) rows[i].SetActive(false);
        if (content is RectTransform rect) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1;
        RefreshDetails();
    }
    public void SelectItem(ItemData item)
    {
        if (item == null || !available.Contains(item)) return;
        SelectedItem = item; SelectedCurrency = Currency.None; Quantity = 1; Refresh();
    }
    public void SelectCurrency(Currency currency)
    {
        if (CurrentMode != Mode.Buy || (currency != Currency.Gold && currency != Currency.MemoryFragments)) return;
        SelectedCurrency = currency; SelectedItem = null; Quantity = 1; Refresh();
    }
    private void ShowCurrencyRow(int index, Currency currency)
    {
        var row = GetRow(index); if (row == null) return;
        row.name = "DevCurrency_" + currency;
        var button = row.GetComponentInChildren<Button>(true);
        if (button != null)
        {
            Set(button.GetComponentInChildren<TMP_Text>(true), CurrencyName(currency) + " 10,000개");
            button.onClick = new Button.ButtonClickedEvent(); button.interactable = true;
            button.onClick.AddListener(() => SelectCurrency(currency));
            TownButtonBinder.GetVisual(button).SetSelected(SelectedCurrency == currency);
            var icon = button.transform.Find("Image")?.GetComponent<Image>();
            if (icon != null)
            {
                var fitter = icon.GetComponent<ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
                icon.sprite = CurrencyIcon(currency); icon.preserveAspect = true; icon.enabled = icon.sprite != null;
            }
        }
        row.SetActive(true);
    }
    private static string CurrencyName(Currency currency) => currency == Currency.Gold ? "골드" : "기억의 파편";
    private Sprite CurrencyIcon(Currency currency) => currency == Currency.Gold ? goldIcon : memoryFragmentIcon;
    private void RefreshDetails()
    {
        bool quest = CurrentMode == Mode.Quest;
        bool currency = CurrentMode == Mode.Buy && SelectedCurrency != Currency.None;
        bool selected = SelectedItem != null || currency;
        long grant = currency ? CurrencyPackSize * Quantity : Quantity;
        if (exchangeControls != null) exchangeControls.SetActive(!quest);
        Set(nameText, SelectedItem != null ? SelectedItem.itemName : quest ? "퀘스트 준비 중" : "판매할 아이템 없음");
        Set(rankText, SelectedItem != null ? SelectedItem.rank + "등급" : "");
        Set(descriptionText, SelectedItem != null ? SelectedItem.description : quest ? "현재 Dev 상점의 퀘스트는 준비 중입니다." : "보유한 아이템이 없습니다.");
        if (thumbnail != null) { thumbnail.sprite = SelectedItem != null ? SelectedItem.thumbnail : null; thumbnail.enabled = thumbnail.sprite != null; }
        long owned = SelectedItem != null && inventory != null ? inventory.GetCount(SelectedItem) : 0;
        if (currency)
        {
            Set(nameText, CurrencyName(SelectedCurrency) + " 10,000개"); Set(rankText, "");
            Set(descriptionText, $"Dev 전용 무료 구매입니다. 수량 1당 {CurrencyName(SelectedCurrency)} 10,000개를 지급합니다.\n지급량 : {grant:N0}개");
            if (thumbnail != null) { thumbnail.sprite = CurrencyIcon(SelectedCurrency); thumbnail.enabled = thumbnail.sprite != null; }
            owned = SelectedCurrency == Currency.Gold ? inventory.Gold : inventory.MemoryFragments;
        }
        Set(quantityText, Quantity.ToString());
        Set(priceText, CurrentMode == Mode.Buy ? "Dev 구매 : 무료" : "Dev 판매 : 보상 없음");
        Set(ownedText, "보유수 : " + owned);
        if (exchangeButton != null)
        {
            Set(exchangeButton.GetComponentInChildren<TMP_Text>(true), CurrentMode == Mode.Buy ? "구매하기" : "판매하기");
            exchangeButton.interactable = !quest && selected &&
                (CurrentMode == Mode.Buy ? owned <= long.MaxValue - grant : owned >= Quantity);
        }
        if (increaseButton != null) increaseButton.interactable = !quest && selected && Quantity < int.MaxValue && (CurrentMode != Mode.Sell || Quantity < owned);
        if (decreaseButton != null) decreaseButton.interactable = !quest && selected && Quantity > 1;
        if (resetButton != null) resetButton.interactable = !quest && selected;
    }
    public void Increase()
    {
        if ((SelectedItem == null && SelectedCurrency == Currency.None) || Quantity == int.MaxValue || (CurrentMode == Mode.Sell && Quantity >= inventory.GetCount(SelectedItem))) return;
        Quantity++; RefreshDetails();
    }
    public void Decrease() { if (Quantity > 1) Quantity--; RefreshDetails(); }
    public void ResetQuantity() { Quantity = 1; RefreshDetails(); }
    public void Exchange()
    {
        if (CurrentMode == Mode.Buy && SelectedCurrency != Currency.None && inventory != null && Quantity > 0)
        {
            long amount = CurrencyPackSize * Quantity;
            inventory.AddCurrency(SelectedCurrency == Currency.Gold ? amount : 0, SelectedCurrency == Currency.MemoryFragments ? amount : 0);
            return;
        }
        if (SelectedItem == null || inventory == null || Quantity < 1) return;
        if (CurrentMode == Mode.Buy) inventory.AddItem(SelectedItem, Quantity);
        else if (CurrentMode == Mode.Sell) inventory.RemoveItem(SelectedItem, Quantity);
    }
    private GameObject GetRow(int index)
    {
        if (content == null || itemTemplate == null) return null;
        while (rows.Count <= index)
        {
            var row = Instantiate(itemTemplate.gameObject, content, false);
            row.SetActive(false); rows.Add(row);
        }
        return rows[index];
    }
    private static void Set(TMP_Text text, string value) { if (text != null) text.text = value; }
}
