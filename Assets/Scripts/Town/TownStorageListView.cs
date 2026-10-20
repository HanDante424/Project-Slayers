using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Displays ownership, never the definition catalogue. Equipped instances remain in this inventory.
public sealed class TownStorageListView : MonoBehaviour
{
    public enum Category { All, Equipment, Materials, Quest }
    [SerializeField] private ItemEquipmentRegistry registry;
    [Header("Existing left list only")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private Transform itemTemplate;
    [SerializeField] private GameObject[] authoredSamples;
    private readonly List<GameObject> rows = new();
    private readonly List<Entry> entries = new();
    private struct Entry
    {
        public UnityEngine.Object Definition;
        public string Name;
        public Sprite Thumbnail;
        public string Key;
        public EquipmentInstance Instance;
    }
    public Category CurrentCategory { get; private set; } = Category.All;
    public UnityEngine.Object SelectedDefinition { get; private set; }
    public string SelectedInstanceId { get; private set; }
    private string selectedKey;
    private SharedInventory inventory;
    public int VisibleCount => entries.Count;

    private void OnEnable() { inventory = InventorySession.Inventory; inventory.Changed += Refresh; Refresh(); }
    private void OnDisable()
    {
        if (inventory != null) inventory.Changed -= Refresh;
        foreach (var row in rows)
            if (row != null) { row.SetActive(false); Destroy(row); }
        rows.Clear(); entries.Clear(); SelectedDefinition = null;
    }
    public void Show(Category category)
    {
        CurrentCategory = category; SelectedDefinition = null;
        if (isActiveAndEnabled) Refresh();
    }
    public void Refresh()
    {
        if (!isActiveAndEnabled) return;
        foreach (var sample in authoredSamples ?? Array.Empty<GameObject>())
            if (sample != null) sample.SetActive(false);
        entries.Clear();
        if (inventory != null)
        {
            if (CurrentCategory == Category.All || CurrentCategory == Category.Equipment)
                foreach (var instance in inventory.Equipment)
                    entries.Add(new Entry { Definition = instance.Definition, Name = instance.Definition.equipmentName + " +" + instance.EnhancementLevel + " [" + instance.InstanceId.Substring(0, 8) + "]", Thumbnail = instance.Definition.thumbnail, Key = "equipment/" + instance.InstanceId, Instance = instance });
            foreach (var item in inventory.Items)
                if (Matches(item.Definition.category))
                    entries.Add(new Entry { Definition = item.Definition, Name = item.Definition.itemName + " ×" + item.Quantity, Thumbnail = item.Definition.thumbnail, Key = "item/" + item.Definition.itemCode });
        }
        if (!entries.Any(entry => entry.Key == selectedKey)) selectedKey = entries.Count > 0 ? entries[0].Key : null;
        var selectedEntry = entries.FirstOrDefault(e => e.Key == selectedKey);
        SelectedDefinition = selectedEntry.Definition; SelectedInstanceId = selectedEntry.Instance?.InstanceId;
        int count = Math.Max(1, entries.Count);
        for (int index = 0; index < count; index++)
        {
            var row = GetRow(index); if (row == null) continue;
            var entry = entries.Count > 0 ? entries[index] : default;
            var button = row.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null) label.text = entry.Definition != null ? entry.Name : "표시할 항목 없음";
                button.onClick = new Button.ButtonClickedEvent();
                button.interactable = entry.Definition != null;
                if (entry.Definition != null)
                {
                    string key = entry.Key;
                    button.onClick.AddListener(() => SelectKey(key));
                }
                TownButtonBinder.GetVisual(button).SetSelected(entry.Definition != null && entry.Key == selectedKey);
                // This is the authored icon inside the list button; the right Desc is intentionally not referenced.
                var icon = button.transform.Find("Image")?.GetComponent<Image>();
                if (icon != null)
                {
                    var fitter = icon.GetComponent<ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
                    icon.sprite = entry.Thumbnail; icon.preserveAspect = true; icon.enabled = entry.Thumbnail != null;
                }
            }
            row.SetActive(true);
        }
        for (int index = count; index < rows.Count; index++) rows[index].SetActive(false);
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }
    public void SelectDefinition(UnityEngine.Object definition)
    {
        if (definition == null || !entries.Any(entry => entry.Definition == definition)) return;
        SelectKey(entries.First(e => e.Definition == definition).Key);
    }
    public void SelectInstance(string id) => SelectKey("equipment/" + id);
    private void SelectKey(string key)
    {
        var entry = entries.FirstOrDefault(e => e.Key == key); if (entry.Definition == null) return;
        selectedKey = key; SelectedDefinition = entry.Definition; SelectedInstanceId = entry.Instance?.InstanceId;
        for (int index = 0; index < entries.Count && index < rows.Count; index++)
        {
            var button = rows[index].GetComponentInChildren<Button>(true);
            if (button != null) TownButtonBinder.GetVisual(button).SetSelected(entries[index].Key == key);
        }
    }
    private bool Matches(ItemCategory category) => CurrentCategory switch
    {
        Category.All => true,
        Category.Equipment => category == ItemCategory.Equipment,
        Category.Materials => category == ItemCategory.Material,
        Category.Quest => category == ItemCategory.Quest,
        _ => false
    };
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
}
