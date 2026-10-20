using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class VerifyEquipmentInventory
{
    [MenuItem("Tools/Slayers/Verify Equipment Inventory %#F7")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Town.unity");
        EditorApplication.playModeStateChanged += Entered;
        EditorApplication.isPlaying = true;
    }
    private static void Entered(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= Entered;
        var root = new GameObject("Equipment inventory verification");
        UnityEngine.Object.DontDestroyOnLoad(root); root.AddComponent<EquipmentInventoryVerification>();
    }
    [MenuItem("Tools/Slayers/Inventory/Grant Selected Recipe Costs (Play Mode)")]
    public static void GrantSelected()
    {
        if (!EditorApplication.isPlaying) return;
        var crafting = UnityEngine.Object.FindFirstObjectByType<TownBlacksmithCraftController>();
        if (crafting?.SelectedEquipment == null) { Debug.LogWarning("Select a blacksmith recipe first."); return; }
        GrantRecipe(crafting.SelectedEquipment, crafting.Quantity);
    }
    public static void GrantRecipe(EquipmentData definition, int quantity)
    {
        if (!EditorApplication.isPlaying || definition == null || quantity < 1) throw new InvalidOperationException("Editor test grants require Play Mode and a selected recipe.");
        var materials = new Dictionary<ItemData, long>();
        foreach (var entry in definition.craftingMaterials)
        {
            if (entry?.material == null || entry.quantity < 1) throw new InvalidOperationException("Invalid material definition.");
            materials.TryGetValue(entry.material, out long count);
            materials[entry.material] = checked(count + (long)entry.quantity * quantity);
        }
        if (!InventorySession.Inventory.AddCurrency(checked((long)definition.craftingGold * quantity), checked((long)definition.craftingMemoryFragments * quantity)))
            throw new InvalidOperationException("Test wallet grant rejected.");
        foreach (var entry in materials)
            if (!InventorySession.Inventory.AddItem(entry.Key, entry.Value)) throw new InvalidOperationException("Test material grant rejected.");
        Debug.Log("[InventoryTestGrant] Exact selected SO recipe costs granted; no production auto-grant.");
    }
}

public sealed class EquipmentInventoryVerification : MonoBehaviour
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private int checks, errors;
    private readonly List<UnityEngine.Object> temporary = new();
    private object Read(object value, string field) => value.GetType().GetField(field, Flags).GetValue(value);
    private Button Button(object value, string field) => (Button)Read(value, field);
    private void Check(bool valid, string label) { checks++; if (!valid) throw new InvalidOperationException(label); }
    private void Near(float value, float expected, string label) => Check(Mathf.Approximately(value, expected), label + ": " + value + " expected " + expected);
    private void Click(Button button)
    {
        Check(button != null && button.IsActive() && button.IsInteractable(), "Active button " + button?.name);
        ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
    }
    private void Clickable(Button button)
    {
        var rect = (RectTransform)button.transform;
        var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
        if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
        {
            for (var parent = button.transform; parent != null; parent = parent.parent)
                Debug.Log("[EquipmentClickDiagnostic] Ancestor " + Path(parent) + " activeSelf=" + parent.gameObject.activeSelf);
            foreach (var graphic in button.transform.parent.GetComponentsInChildren<Graphic>(true))
                Debug.Log("[EquipmentClickDiagnostic] " + Path(graphic.transform) + " active=" + graphic.isActiveAndEnabled + " raycast=" + graphic.raycastTarget + " depth=" + graphic.depth + " culled=" + graphic.canvasRenderer.cull + " rect=" + ((RectTransform)graphic.transform).rect);
            foreach (var mask in button.GetComponentsInParent<RectMask2D>(true))
                Debug.Log("[EquipmentClickDiagnostic] Mask " + Path(mask.transform) + " rect=" + mask.canvasRect);
        }
        Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
            "UI raycast reaches " + Path(button.transform) + " at " + point + " (hits=" + string.Join(";", hits.Take(6).Select(h => Path(h.gameObject.transform))) + ")");
    }
    private static string Path(Transform value) => value.parent != null ? Path(value.parent) + "/" + value.name : value.name;
    private void Log(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private IEnumerator Start()
    {
        Application.logMessageReceived += Log;
        var work = Body(); bool failed = false;
        while (true)
        {
            object next;
            try { if (!work.MoveNext()) break; next = work.Current; }
            catch (Exception exception) { failed = true; Debug.LogError("[EquipmentInventoryTest] FAIL " + exception); break; }
            yield return next;
        }
        foreach (var value in temporary) if (value != null) Destroy(value);
        InventorySession.Inventory.Clear();
        Application.logMessageReceived -= Log;
        Debug.Log($"[EquipmentInventoryTest] RESULT checks={checks}, errors={errors}, failed={failed}");
        EditorApplication.delayCall += () => EditorApplication.isPlaying = false;
    }
    private IEnumerator Body()
    {
        yield return null; yield return null;
        var inventory = InventorySession.Inventory;
        Check(inventory.Items.Count == 0 && inventory.Equipment.Count == 0 && inventory.Gold == 0 && inventory.MemoryFragments == 0, "No automatic test grants");
        var registry = FindFirstObjectByType<ItemEquipmentRegistry>();
        var definitions = registry.GetAllEquipment(); var itemDefinitions = registry.GetAllItems();
        Check(definitions.Count == 3 && itemDefinitions.Count == 4, "Authored 3 equipment / 4 materials");
        var beforeAssets = definitions.ToDictionary(d => d, d => EditorJsonUtility.ToJson(d));
        var bracelet = definitions.Single(d => d.slot == EquipmentSlot.Bracelet);
        var belt = definitions.Single(d => d.slot == EquipmentSlot.Belt);
        var gloves = definitions.Single(d => d.slot == EquipmentSlot.Gloves);
        Check(!inventory.TryCraft(bracelet, 1) && inventory.Equipment.Count == 0 && inventory.Gold == 0, "Insufficient funds have no partial effects");
        var menu = FindFirstObjectByType<TownMenuController>();
        // Isolated Dev-shop fixture, cleared before the existing recipe/equipment scenarios.
        menu.OpenShop();
        var devNavigation = FindFirstObjectByType<TownShopNavigationController>();
        var devButton = Button(devNavigation, "devButton");
        yield return new WaitForEndOfFrame(); Clickable(devButton); Click(devButton);
        var devShop = FindFirstObjectByType<TownDevShopController>();
        Check(devShop != null && devShop.VisibleCount == itemDefinitions.Count + 2 && devShop.SelectedItem == itemDefinitions[0], "Dev buy lists all ItemData plus two currency packs and selects first item");
        var title = FindFirstObjectByType<TownTitleController>();
        Check(((TMPro.TMP_Text)Read(title, "titleText")).text == "Dev - 구매", "Dev buy title");
        foreach (var item in itemDefinitions)
        {
            devShop.SelectItem(item);
            Check(((TMPro.TMP_Text)Read(devShop, "nameText")).text == item.itemName &&
                ((TMPro.TMP_Text)Read(devShop, "descriptionText")).text == item.description &&
                ((Image)Read(devShop, "thumbnail")).sprite == item.thumbnail, "Dev item details use actual SO " + item.itemName);
            Click(Button(devShop, "increaseButton")); Click(Button(devShop, "increaseButton"));
            Check(devShop.Quantity == 3, "Dev quantity increments");
            yield return new WaitForEndOfFrame(); Clickable(Button(devShop, "exchangeButton"));
            Click(Button(devShop, "exchangeButton"));
            Check(inventory.GetCount(item) == 3 && inventory.Gold == 0 && inventory.MemoryFragments == 0, "Dev purchase grants exact quantity for free " + item.itemName);
            Click(Button(devShop, "resetButton")); Check(devShop.Quantity == 1, "Dev reset quantity");
        }
        devNavigation.OpenQuest();
        Check(devShop.CurrentMode == TownDevShopController.Mode.Quest && devShop.SelectedItem == null &&
            !((GameObject)Read(devShop, "exchangeControls")).activeSelf, "Quest is explicit placeholder with no exchange");
        devShop.Exchange(); Check(inventory.Items.Count == itemDefinitions.Count, "Quest cannot grant or remove inventory");
        devNavigation.OpenSell();
        Check(devShop.VisibleCount == itemDefinitions.Count && devShop.SelectedItem == itemDefinitions[0], "Dev sell shows owned items and selects first");
        foreach (var item in itemDefinitions)
        {
            devShop.SelectItem(item); Click(Button(devShop, "increaseButton")); Click(Button(devShop, "increaseButton"));
            Check(!Button(devShop, "increaseButton").interactable && devShop.Quantity == 3, "Sell quantity stops at ownership");
            Click(Button(devShop, "exchangeButton"));
            Check(inventory.GetCount(item) == 0 && inventory.Gold == 0 && inventory.MemoryFragments == 0, "Dev sale removes quantity without reward " + item.itemName);
        }
        Check(devShop.VisibleCount == 0 && devShop.SelectedItem == null && !Button(devShop, "exchangeButton").interactable, "Empty sell clears stale selection and disables action");
        devNavigation.OpenBuy();
        var devRows = (List<GameObject>)Read(devShop, "rows");
        foreach (var currency in new[] { TownDevShopController.Currency.Gold, TownDevShopController.Currency.MemoryFragments })
        {
            var currencyButton = devRows.Single(r => r.activeSelf && r.name == "DevCurrency_" + currency).GetComponentInChildren<Button>();
            yield return new WaitForEndOfFrame(); Clickable(currencyButton); Click(currencyButton);
            Check(devShop.SelectedCurrency == currency && devShop.SelectedItem == null && devShop.Quantity == 1,
                "Currency row selects explicit currency, resets quantity");
            Check(((Image)Read(devShop, "thumbnail")).sprite != null, "Currency uses existing icon");
            long beforeGold = inventory.Gold, beforeMemory = inventory.MemoryFragments;
            Click(Button(devShop, "exchangeButton"));
            Check(inventory.Gold == beforeGold + (currency == TownDevShopController.Currency.Gold ? 10000 : 0) &&
                inventory.MemoryFragments == beforeMemory + (currency == TownDevShopController.Currency.MemoryFragments ? 10000 : 0), "Single currency purchase grants exactly 10000 of only selected currency");
            Click(Button(devShop, "increaseButton")); Click(Button(devShop, "exchangeButton"));
            Check(inventory.Gold == beforeGold + (currency == TownDevShopController.Currency.Gold ? 30000 : 0) &&
                inventory.MemoryFragments == beforeMemory + (currency == TownDevShopController.Currency.MemoryFragments ? 30000 : 0), "Quantity two grants 20000 additional currency");
            Check(inventory.Items.Count == 0, "Currency is wallet balance, not an item stack");
        }
        devNavigation.OpenSell();
        Check(devShop.VisibleCount == 0 && devShop.SelectedCurrency == TownDevShopController.Currency.None, "Currencies never appear in sell list");
        Check(inventory.SpendCurrency(30000, 30000), "Currency test fixture cleanup");
        for (int i = 0; i < 3; i++) { devNavigation.OpenBlackMarket(); Click(devButton); }
        devShop.SelectItem(itemDefinitions[0]);
        Click(Button(devShop, "increaseButton")); Click(Button(devShop, "decreaseButton"));
        Check(devShop.Quantity == 1, "Dev quantity decrease");
        Click(Button(devShop, "exchangeButton"));
        Check(inventory.GetCount(itemDefinitions[0]) == 1, "Dev re-entry does not duplicate listeners");
        menu.OpenStorage();
        Check(FindFirstObjectByType<TownStorageListView>().VisibleCount == 1, "Dev purchase immediately available in shared storage");
        inventory.Clear();
        menu.OpenStorage(); var storage = FindFirstObjectByType<TownStorageListView>();
        Check(storage.VisibleCount == 0, "Definition catalogue is not owned inventory");
        menu.OpenShop(); var shop = FindFirstObjectByType<TownShopNavigationController>();
        shop.OpenBlackSmith(); shop.OpenBracelet();
        var craft = FindFirstObjectByType<TownBlacksmithCraftController>();
        Check(craft.SelectedEquipment == bracelet && !Button(craft, "craftButton").interactable, "Real blacksmith recipe / cost gate");
        Click(Button(craft, "increaseButton")); Click(Button(craft, "increaseButton"));
        Check(craft.Quantity == 3, "Craft quantity controls select three");
        VerifyEquipmentInventory.GrantRecipe(bracelet, 3);
        yield return new WaitForEndOfFrame(); Clickable(Button(craft, "craftButton"));
        Click(Button(craft, "craftButton"));
        Check(inventory.Equipment.Count == 3 && inventory.Gold == 0 && inventory.MemoryFragments == 0 && inventory.Items.Count == 0, "Craft button atomically consumes exact wallet/material costs and grants 3 instances");
        Check(inventory.Equipment.Select(i => i.InstanceId).Distinct().Count() == 3 && inventory.Equipment.All(i => i.EnhancementLevel == 0 && i.AdditionalModifiers.Count == 0 && i.AdditionalSets.Count == 0), "Unique IDs and pristine instance upgrades");
        Check(!inventory.TryCraft(bracelet, 1), "Cannot craft again after spending costs");
        menu.OpenStorage(); storage = FindFirstObjectByType<TownStorageListView>();
        storage.Show(TownStorageListView.Category.Equipment);
        Check(storage.VisibleCount == 3, "Three same-definition instances in storage");
        var shared = inventory.Equipment[0]; storage.SelectInstance(shared.InstanceId);
        Check(storage.SelectedInstanceId == shared.InstanceId, "Storage selects exact instance, not just SO");
        Check(inventory.AddItem(itemDefinitions[0], 2) && inventory.AddItem(itemDefinitions[0], 3) && inventory.GetCount(itemDefinitions[0]) == 5, "Normal item stacks merge");
        storage.Show(TownStorageListView.Category.Materials); Check(storage.VisibleCount == 1, "Storage lists only owned material types");
        Check(inventory.RemoveItem(itemDefinitions[0], 5) && inventory.GetCount(itemDefinitions[0]) == 0, "Normal item consumption");
        menu.OpenCharacter();
        var characterView = FindFirstObjectByType<TownCharacterStatsController>(); characterView.SelectMayhem();
        var source = FindFirstObjectByType<InventoryCharacterDataSource>();
        var mayhem = characterView.CurrentCharacter;
        var identities = (InventoryCharacterDataSource.CharacterIdentity[])Read(source, "characters");
        var ern = identities.Single(c => c.character != mayhem).character;
        var mayhemStats = source.GetStats(mayhem); var ernStats = source.GetStats(ern);
        var buff = new StatModifier(StatType.Health, StatModifierType.Flat, 10f, "verification/buff");
        mayhemStats.AddModifier(buff); float baseHp = mayhemStats.MaxHealth + 10;
        var nav = FindFirstObjectByType<TownCharacterNavigationController>(); nav.OpenEquip(); nav.OpenBraceletSelection();
        var equipmentView = FindFirstObjectByType<TownCharacterEquipmentController>();
        var owned = source.GetOwned(mayhem, "Bracelet"); Check(owned.Count == 3, "Equipment UI data exposes all separate owned instances");
        Check(source.GetOwned(mayhem, "Belt").Count == 0 && !source.CanEquip(mayhem, "Belt", owned[0]), "Slot mismatch rejected");
        yield return new WaitForEndOfFrame();
        var ownedRows = (List<GameObject>)Read(equipmentView, "ownedRows");
        Check(ownedRows.Count == 3, "Three owned equipment rows");
        Clickable(ownedRows[0].GetComponentInChildren<Button>(true));
        Click(ownedRows[0].GetComponentInChildren<Button>(true));
        yield return new WaitForEndOfFrame(); Clickable(Button(equipmentView, "equipButton"));
        Click(Button(equipmentView, "equipButton"));
        Near(mayhemStats.FinalMaxHealth, baseHp * 1.05f, "Equipping applies SO percentage and preserves external buff");
        Check(source.GetEquipped(mayhem, "Bracelet").Instance == shared, "UI equips selected instance ID");
        var selectedView = (TownCharacterEquipmentController.ItemView)Read(equipmentView, "selectedView");
        Click(selectedView.setButton); Check(((GameObject)Read(equipmentView, "setPopup")).activeInHierarchy, "Selected equipment set popup opens");
        yield return new WaitForEndOfFrame(); Clickable(Button(equipmentView, "popupClose")); Click(Button(equipmentView, "popupClose"));
        for (int i = 0; i < 4; i++)
        {
            Click(Button(equipmentView, "equipButton")); Near(mayhemStats.FinalMaxHealth, baseHp, "Unequip restores base+buff");
            Click(Button(equipmentView, "equipButton")); Near(mayhemStats.FinalMaxHealth, baseHp * 1.05f, "Re-equip does not accumulate modifiers");
        }
        for (int i = 0; i < 3; i++)
        {
            Check(source.TryEquip(mayhem, "Bracelet", owned[1]) && source.GetEquipped(mayhem, "Bracelet") == owned[1], "Replace with separate same-SO instance");
            Check(source.TryEquip(mayhem, "Bracelet", owned[0]), "Replace back to original instance");
            Near(mayhemStats.FinalMaxHealth, baseHp * 1.05f, "Replacement never accumulates old effects");
            Check(mayhemStats.ModifierCount == 3, "Only two SO modifiers plus external buff remain");
        }
        var mayhemId = InventorySession.GetCharacterId(mayhem); var ernId = InventorySession.GetCharacterId(ern);
        Check(InventorySession.Equipment.Equip(ernId, EquipmentSlot.Bracelet, shared), "Both characters may wear same instance");
        Near(ernStats.FinalMaxHealth, ernStats.MaxHealth * 1.05f, "Shared instance applies to character B");
        Check(InventorySession.Equipment.Unequip(mayhemId, EquipmentSlot.Bracelet), "Character A unequips independently");
        Near(ernStats.FinalMaxHealth, ernStats.MaxHealth * 1.05f, "Character B unaffected by A unequip");
        Check(shared.SetEnhancementLevel(25) && owned[0].UpgradeText == "+25", "Enhancement count +25 remains on instance");
        Check(shared.AddModifier(StatType.Attack, StatModifierType.Flat, 2) && shared.AddModifier(StatType.Attack, StatModifierType.Flat, 2), "Duplicate additional options allowed");
        Near(ernStats.FinalAttack, (ernStats.Attack + 4) * 1.05f, "Both duplicate options apply to all wearers");
        Check(source.TryEquip(mayhem, "Bracelet", owned[1]), "Comparison fixture equips a separate instance");
        var replacement = source.GetReplacementStats(mayhem, "Bracelet", owned[0]);
        Near(replacement.FinalAttack, (mayhemStats.Attack + 4) * 1.05f, "Replacement snapshot includes selected instance extras");
        Near(replacement.FinalMaxHealth, baseHp * 1.05f, "Replacement snapshot preserves unrelated buff");
        Near(source.GetStatsWithoutEquipment(mayhem).FinalMaxHealth, baseHp, "Without-equipment snapshot preserves unrelated buff");
        Near(mayhemStats.FinalAttack, mayhemStats.Attack * 1.05f, "Comparison does not mutate live stats");
        Check(source.TryUnequip(mayhem, "Bracelet"), "Comparison fixture cleanup");
        Check(!shared.AddSet(bracelet.set), "Cannot duplicate base set as additional set");
        var extraSet = ScriptableObject.CreateInstance<EquipmentSetData>(); temporary.Add(extraSet);
        Check(shared.AddSet(extraSet) && !shared.AddSet(extraSet), "Different additional set retained exactly once");
        var extraSet2 = ScriptableObject.CreateInstance<EquipmentSetData>(); temporary.Add(extraSet2);
        Check(shared.AddSet(extraSet2) && shared.AdditionalSets.Count == 2, "Multiple distinct additional sets supported");
        foreach (var definition in new[] { belt, gloves })
        {
            VerifyEquipmentInventory.GrantRecipe(definition, 1); Check(inventory.TryCraft(definition, 1), "Craft other real recipe " + definition.equipmentName);
            Check(InventorySession.Equipment.Equip(ernId, definition.slot, inventory.Equipment.Last()), "Equip real set member");
        }
        Near(ernStats.FinalMaxHealth, ernStats.MaxHealth * 1.2f, "3 equipment health bonuses plus active 3-piece set (additive 20%)");
        Check(source.GetSets(ern).Single(s => s.Definition == bracelet.set).Active, "Actual set threshold activates");
        Check(InventorySession.Equipment.Unequip(ernId, EquipmentSlot.Gloves), "Remove set member");
        Near(ernStats.FinalMaxHealth, ernStats.MaxHealth * 1.1f, "Set threshold deactivates without lingering modifiers");
        menu.OpenStorage(); storage = FindFirstObjectByType<TownStorageListView>(); storage.Show(TownStorageListView.Category.Equipment);
        Check(storage.VisibleCount == 5, "Equipped equipment remains in common storage");
        var snapshot = InventorySession.Capture(s => s == extraSet ? "verification/additional-set" : s == extraSet2 ? "verification/additional-set-2" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(s)));
        Check(snapshot.equipment.Count == 5 && snapshot.equipped.Any(e => e.characterId == ernId && e.instanceId == shared.InstanceId) && snapshot.equipment.Single(e => e.instanceId == shared.InstanceId).additionalModifiers.Count == 2, "Save DTO stores codes, IDs, upgrades, duplicate options and loadout links");
        Check(inventory.RemoveEquipment(shared.InstanceId) && InventorySession.Equipment.GetEquipped(ernId, EquipmentSlot.Bracelet) == null, "Removing owned instance cleans dangling loadout references");
        Near(ernStats.FinalAttack, ernStats.Attack, "Removed instance effects cleaned");
        var isolated = new SharedInventory();
        Check(isolated.AddCurrency(long.MaxValue, long.MaxValue) && !isolated.AddCurrency(1, 1) && isolated.Gold == long.MaxValue, "Wallet overflow leaves previous state");
        Check(isolated.AddItem(itemDefinitions[0], long.MaxValue) && !isolated.AddItem(itemDefinitions[0], 1) && isolated.GetCount(itemDefinitions[0]) == long.MaxValue, "Item overflow rejected without mutation");
        var missingMaterials = new SharedInventory();
        missingMaterials.AddCurrency(bracelet.craftingGold, bracelet.craftingMemoryFragments);
        missingMaterials.AddItem(bracelet.craftingMaterials[0].material, bracelet.craftingMaterials[0].quantity);
        Check(!missingMaterials.TryCraft(bracelet, 1) && missingMaterials.Gold == bracelet.craftingGold && missingMaterials.MemoryFragments == bracelet.craftingMemoryFragments &&
            missingMaterials.GetCount(bracelet.craftingMaterials[0].material) == bracelet.craftingMaterials[0].quantity && missingMaterials.Equipment.Count == 0,
            "Missing one material consumes no other material or currency");
        var duplicateRecipe = Instantiate(bracelet); temporary.Add(duplicateRecipe);
        duplicateRecipe.craftingMaterials = new List<EquipmentCraftingMaterial> {
            new() { material = itemDefinitions[0], quantity = 3 }, new() { material = itemDefinitions[0], quantity = 4 } };
        long beforeStack = isolated.GetCount(itemDefinitions[0]);
        Check(isolated.TryCraft(duplicateRecipe, 1) && isolated.GetCount(itemDefinitions[0]) == beforeStack - 7, "Repeated recipe material rows aggregate before consumption");
        duplicateRecipe.craftingMaterials = new List<EquipmentCraftingMaterial> {
            new() { material = itemDefinitions[0], quantity = int.MaxValue }, new() { material = itemDefinitions[0], quantity = int.MaxValue }, new() { material = itemDefinitions[0], quantity = int.MaxValue } };
        var overflowInventory = new SharedInventory(); overflowInventory.AddCurrency(long.MaxValue, long.MaxValue); overflowInventory.AddItem(itemDefinitions[0], long.MaxValue);
        Check(!overflowInventory.TryCraft(duplicateRecipe, int.MaxValue) && overflowInventory.Gold == long.MaxValue && overflowInventory.GetCount(itemDefinitions[0]) == long.MaxValue && overflowInventory.Equipment.Count == 0,
            "Aggregated material multiplication overflow rejects atomically");
        duplicateRecipe.craftingGold = duplicateRecipe.craftingMemoryFragments = 0; duplicateRecipe.craftingMaterials.Clear();
        Check(!isolated.CanCraft(duplicateRecipe, 1), "Unspecified recipe is not silently free");
        var roundTrip = JsonUtility.FromJson<InventoryRecord>(JsonUtility.ToJson(snapshot));
        Check(roundTrip.equipment.Single(e => e.instanceId == shared.InstanceId).additionalSetIds.Count == 2 && roundTrip.equipped.Any(e => e.instanceId == shared.InstanceId),
            "Value-only snapshot serializes without SO copies");
        Check(!inventory.TryCraft(bracelet, int.MaxValue) && !inventory.TryCraft(bracelet, -1), "Excessive/invalid quantity rejected before allocation or consumption");
        // Ephemeral copies exercise gauge/set math without modifying any authored balance assets.
        var gaugeDefinition = Instantiate(bracelet); temporary.Add(gaugeDefinition); gaugeDefinition.additionalStaggerGaugePercent = 30;
        var gaugeItem = new EquipmentInstance(gaugeDefinition);
        var gaugeRoot = new GameObject("Gauge verification"); gaugeRoot.SetActive(false); temporary.Add(gaugeRoot);
        var gaugeStats = gaugeRoot.AddComponent<CharacterStats>(); gaugeStats.Initialize(ern.statsData, 1);
        EquipmentStatEvaluator.Apply(gaugeStats, new[] { gaugeItem });
        Near(gaugeStats.FinalStaggerGaugeHealthPercent, ern.statsData.staggerGaugeHealthPercent * 1.3f, "Gauge relative-percent conversion");
        Near(gaugeStats.FinalStaggerGauge, gaugeStats.FinalMaxHealth * ern.statsData.staggerGaugeHealthPercent * 1.3f / 100f, "Gauge uses final HP and adjusted ratio");
        var gaugeSet = ScriptableObject.CreateInstance<EquipmentSetData>(); temporary.Add(gaugeSet);
        gaugeSet.bonuses.Add(new EquipmentSetBonus { requiredEquipmentCount = 1, additionalStaggerGaugePercent = 10 });
        Check(gaugeItem.AddSet(gaugeSet), "Test-only additional set gauge fixture");
        EquipmentStatEvaluator.Apply(gaugeStats, new[] { gaugeItem });
        Near(gaugeStats.FinalStaggerGaugeHealthPercent, ern.statsData.staggerGaugeHealthPercent * 1.4f, "SO and set gauge percentages combine before relative conversion");
        foreach (var definition in definitions) Check(EditorJsonUtility.ToJson(definition) == beforeAssets[definition], "Authored SO unchanged " + definition.name);
        Debug.Log("[EquipmentInventoryTest] Real recipes, costs, 4 material types, ownership UI, equip/unequip, shared wearers, duplicates, sets, gauge, overflow and DTO verified.");
        // Session ownership survives actual scene reload; the Town adapter rebuilds stats from the same IDs.
        int expectedCount = inventory.Equipment.Count;
        SceneManager.LoadScene("Town"); yield return null; yield return null;
        Check(ReferenceEquals(InventorySession.Inventory, inventory) && inventory.Equipment.Count == expectedCount, "Ownership persists across Town scene load");
        FindFirstObjectByType<TownBattleNavigationController>().OpenStageSelect();
        var selection = FindFirstObjectByType<TownCombatCharacterSelectController>();
        selection.OpenSelectedStage(); selection.SelectErn();
        var battleEquipmentFields = (TownCombatCharacterSelectController.EquipmentField[])Read(selection, "equipmentFields");
        Check(battleEquipmentFields.Single(f => f.slot == EquipmentSlot.Belt).text.text.Contains(belt.equipmentName), "Combat selection displays actually equipped belt");
        selection.EnterCombat();
        yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "Dev_Combat", "Equipped character enters real combat scene");
        var entry = FindFirstObjectByType<CombatPlayerEntry>();
        Check(entry.SelectedCharacter == ern, "Combat receives equipped character identity");
        var combatStats = entry.ActivePlayer.GetComponent<CharacterStats>();
        Near(combatStats.FinalMaxHealth, combatStats.MaxHealth * 1.05f, "Combat actor receives owned belt stats before activation");
        Check(entry.ActivePlayer.GetComponent<Health>().MaxHealth == Mathf.RoundToInt(combatStats.FinalMaxHealth), "Existing player HP initialization uses equipped final health");
        SceneManager.LoadScene("Town"); yield return null; yield return null;
        menu = FindFirstObjectByType<TownMenuController>();
        source = FindFirstObjectByType<InventoryCharacterDataSource>();
        var resetStats = source.GetStats(ern);
        resetStats.AddModifier(new StatModifier(StatType.Health, StatModifierType.Flat, 10f, "verification/reset-buff"));
        var savedCharacter = TownCharacterSelection.Character;
        int savedLevel = TownCharacterSelection.Level;
        var loadouts = InventorySession.Equipment;
        Check(inventory.AddCurrency(123, 456) && inventory.AddItem(itemDefinitions[0], 7), "Reset fixture has currencies and items");
        Check(loadouts.GetEquipped(ernId).Count > 0 && inventory.Equipment.Count > 0, "Reset fixture has owned and worn equipment");
        var gameSession = GameSession.Instance;
        if (gameSession == null)
        {
            var sessionRoot = new GameObject("Settings verification session"); temporary.Add(sessionRoot);
            gameSession = sessionRoot.AddComponent<GameSession>();
            gameSession.Data.testSkillUnlocked = true;
            gameSession.Data.stage1Cleared = true;
            gameSession.Data.controlBindingsJson = "settings-verification-binding-sentinel";
        }
        bool savedUnlocked = false, savedCleared = false;
        string savedBindings = null;
        if (gameSession != null)
        {
            gameSession.Data.testCurrency = 99;
            savedUnlocked = gameSession.Data.testSkillUnlocked;
            savedCleared = gameSession.Data.stage1Cleared;
            savedBindings = gameSession.Data.controlBindingsJson;
        }
        var option = Button(menu, "optionButton");
        Check(option.onClick.GetPersistentEventCount() == 1 && option.onClick.GetPersistentTarget(0) == menu &&
            option.onClick.GetPersistentMethodName(0) == nameof(TownMenuController.OpenOption), "Settings has exactly one persistent OpenOption event");
        yield return new WaitForEndOfFrame(); Clickable(option); Click(option);
        var settings = FindFirstObjectByType<TownSettingsController>();
        Check(settings != null && settings.isActiveAndEnabled && !((GameObject)Read(menu, "battlePanel")).activeSelf, "Settings button opens authored panel, not battle");
        title = (TownTitleController)Read(settings, "titleController");
        Check(((TMPro.TMP_Text)Read(title, "titleText")).text == "설정", "Settings title");
        for (int i = 0; i < 3; i++) { menu.OpenStorage(); Click(option); }
        var resetButton = Button(settings, "resetDataButton");
        Check(resetButton.onClick.GetPersistentEventCount() == 0, "Reset button has no unrelated persistent events");
        yield return new WaitForEndOfFrame(); Clickable(resetButton);
        int notifications = 0;
        Action observedReset = () => notifications++;
        inventory.Changed += observedReset;
        Click(resetButton);
        inventory.Changed -= observedReset;
        Check(notifications == 1, "Panel re-entry does not duplicate reset listeners");
        Check(ReferenceEquals(inventory, InventorySession.Inventory) && ReferenceEquals(loadouts, InventorySession.Equipment), "Reset retains shared objects and subscribers");
        Check(inventory.Gold == 0 && inventory.MemoryFragments == 0 && inventory.Items.Count == 0 && inventory.Equipment.Count == 0,
            "Settings clears both currencies, every item and every equipment instance");
        Check(loadouts.Capture().Count == 0, "Reset clears all characters' equipment links");
        Near(resetStats.FinalMaxHealth, resetStats.MaxHealth + 10f, "Reset removes equipment stats, preserves unrelated buff");
        Check(TownCharacterSelection.Character == savedCharacter && TownCharacterSelection.Level == savedLevel, "Reset preserves character selection/level");
        if (gameSession != null)
            Check(gameSession.Data.testCurrency == 0 && gameSession.Data.testSkillUnlocked == savedUnlocked &&
                gameSession.Data.stage1Cleared == savedCleared && gameSession.Data.controlBindingsJson == savedBindings, "Reset only clears legacy currency, preserves other game data");
        Click(resetButton);
        Check(settings.isActiveAndEnabled && inventory.Equipment.Count == 0, "Empty reset is safe and stays on settings");
        menu.OpenStorage();
        Check(FindFirstObjectByType<TownStorageListView>().VisibleCount == 0, "Storage refreshes empty after reset");
    }
}
