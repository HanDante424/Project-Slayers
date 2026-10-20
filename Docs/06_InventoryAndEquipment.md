# Inventory and equipment integration

Status: implementation of the explicit equipment-instance task, 2026-10-20.
Balance definitions remain owned by the existing SO assets and the reference sheet.
Reference: [슬레이어즈 스탯 1.0](https://docs.google.com/spreadsheets/d/1RG3vj2PYT7M5ndVQSoqgtrXC_eHaqDN-ODn--ljIRCA/edit).
The registered equipment rows 000..002 and item rows 000..003 were compared read-only with the existing SO assets.

## Ownership and flow

- `ItemEquipmentRegistry`: definition catalogue only; never ownership or quantities.
- `InventorySession`: in-memory session lifetime, shared across scenes. Starts empty.
- `SharedInventory`: item-code stacks, independent equipment instances, gold and memory fragments.
- `EquipmentInstance`: immutable unique ID and definition reference, mutable enhancement count, duplicate-permitted additional modifiers, distinct additional sets excluding the base set.
- `CharacterEquipment`: stable character ID -> slot -> owned instance ID. An instance can be worn by multiple characters concurrently.
- Town's `InventoryCharacterDataSource.characters` explicitly maps existing character SO GUIDs to stable IDs. Display names and array indices are not identities.

Craft button -> `InventoryCraftingDataSource` -> full recipe validation and construction of the next inventory state -> one commit/notification -> storage and equipment views refresh.
An entirely unspecified recipe (no currency costs and no materials) is not silently made free. No fallback costs are generated.

Equip/unequip -> `CharacterEquipment` -> `CharacterEquipmentBinding` -> `EquipmentStatEvaluator` -> replace only `equipment/` modifiers in `CharacterStats` -> existing stat views refresh.
The character data-source adapter supplies stable instance wrappers and comparison snapshots retaining the same non-equipment modifiers.
The existing equip button becomes **장착 해제** when its selected instance is already equipped.

All reached set thresholds contribute their defined bonuses. A piece contributes at most once per set, but can contribute to multiple distinct sets.
The gauge uses final HP and `base gauge ratio * (1 + (equipment + active-set additional gauge percent) / 100)`.
No enhancement rewards, probabilities, or per-level growth are implemented.

## Lifetime and future persistence

Scene changes preserve session ownership. New-game creation clears it. Entering a fresh Play session starts empty.
`InventorySession.Capture` creates value-only records containing definition codes, instance IDs, enhancement counts, modifier values, additional set IDs, character IDs and equipped instance IDs.
The caller supplies a stable set-ID resolver. SO objects are not copied into these records.
Disk save, restore/migration, cloud synchronization and production reward grants remain outside this implementation.
Existing `SaveManager` saves do not yet persist or restore this inventory.

## Town settings reset

The authored Settings button opens `OptionPanel` through `TownMenuController.OpenOption`.
`TownSettingsController` binds the existing data-reset button while the panel is enabled and unbinds it when disabled.
It clears gold, memory fragments, item stacks and equipment instances in the existing `SharedInventory` object.
The normal inventory notification removes all characters' equipped-instance references and equipment-derived modifiers.
Unrelated stat modifiers, character selection/level, key bindings, skill unlocks and stage flags are preserved.
The legacy `GameData.testCurrency` is also zeroed when a `GameSession` exists; no new game or save deletion is performed.
This is a session reset, not a disk-save reset. The existing inventory has no disk restoration yet.

Verification: Unity compilation executed; the extended equipment/settings Play Mode check passed 131 checks with 0 errors.
It exercises the real settings/reset button events and UI raycasts, repeat entry, a single reset notification, empty repeat reset,
all-character equipment cleanup, wallet/stacks/instances, storage refresh and preservation of non-equipment state.

## Dev shop

The user-authored `ShopPanel/DevPanel` uses `TownDevShopController`; the existing shop navigation exposes Dev Buy/Sell/Quest.
Confirmed development behavior: buy grants the selected quantity for free; sell removes owned ItemData stacks with no reward; quest displays a preparation notice and cannot exchange items.
Buy is sourced from all current `ItemEquipmentRegistry.GetAllItems()` registrations (currently four), not hard-coded item names or a separate catalogue.
Dev Buy also includes explicit Gold and Memory Fragment currency packs, using the existing currency icons.
Each pack grants 10,000 of its currency for free; quantity is the number of packs (quantity 2 grants 20,000).
Currency goes through `SharedInventory.AddCurrency`, not ItemData stacks, and never appears in the sell list.
The wallet's existing overflow checks are retained; no SO definitions or production economy prices are added.
EquipmentData definitions remain separate and are not added to this ItemData catalogue.
The first available item is selected on each menu entry. Detail sprites, names, numeric ranks, descriptions and ownership come from the existing definitions/session.
The existing controls adjust/reset quantity. Selling is bounded by current ownership and an empty inventory disables the exchange.
Shared inventory notifications update storage and prevent duplicate callbacks after re-entry. No definition SO, price data or production quest system is added.
An existing empty description content object receives TMP/CanvasRenderer components using the authored name label's font settings; existing RectTransforms are unchanged.
Unity compilation executed successfully; the expanded Play Mode verification passed 204 checks with zero errors, including all four items, real button events/raycasts and existing equipment/settings regression.
Currency-pack follow-up: Unity compilation executed and the extended Play Mode check passed 226 checks with zero errors, including exact single/multiple-pack grants and currency separation.

## Editor-only verification

- `Tools > Slayers > Verify Equipment Inventory` (`Ctrl+Shift+F7`): runs in Play Mode against the existing Town scene and registered assets; grants are test-fixture-only.
- `Tools > Slayers > Inventory > Grant Selected Recipe Costs (Play Mode)`: grants exactly the SO costs for the selected blacksmith recipe and quantity. Never runs automatically.
- `Tools > Slayers > Verify Character Entry` remains the combat-entry regression check.

Unity compilation and Play Mode outcomes must be reported from actual runs, not inferred from this document.

## File manifest

Created (each new C# script includes a new `.meta`, without changing existing GUIDs):

- `Assets/Scripts/System/Data/EquipmentInstance.cs`
- `Assets/Scripts/System/Data/SharedInventory.cs`
- `Assets/Scripts/System/Data/CharacterEquipment.cs`
- `Assets/Scripts/System/Data/InventorySession.cs`
- `Assets/Scripts/System/Character/EquipmentStatEvaluator.cs`
- `Assets/Scripts/System/Character/CharacterEquipmentBinding.cs`
- `Assets/Scripts/Town/InventoryCraftingDataSource.cs`
- `Assets/Scripts/Town/InventoryCharacterDataSource.cs`
- `Assets/Editor/VerifyEquipmentInventory.cs`
- This document.

Modified:

- `Assets/Scenes/Town.unity`: two adapters on the existing registry object, explicit character identities, data-source references, existing combat equipment text bindings. Broken legacy stat-icon references use the corresponding existing `IconRegistry` sprites. No layout definition changes.
- `Assets/Scripts/System/Character/CharacterStats.cs`: namespaced modifier replacement, runtime comparison copying, additional gauge-ratio modifiers; original flat/percent stat formula retained.
- `Assets/Scripts/System/Game/GameSession.cs`: new game clears session ownership.
- `Assets/Scripts/Town/TownCharacterDataSource.cs`: instance-backed presentation wrapper and unequip hook.
- `Assets/Scripts/Town/TownCharacterEquipmentController.cs`: instance rows, upgrades, comparison, equip/unequip, active ownership scroll.
- `Assets/Scripts/Town/TownStorageListView.cs`: owned stacks/instances rather than registered definitions.
- `Assets/Scripts/Town/TownCombatCharacterSelectController.cs`: equipped-stat and equipment preview.
- `Assets/Scripts/Player/CombatPlayerEntry.cs`: apply equipment before actor activation.
- `Assets/Scripts/System/Data/EquipmentData.cs`, `EquipmentSetData.cs`: update now-obsolete runtime tooltips/comments only.
- `Assets/Editor/VerifyCharacterEntry.cs`: verify the authoritative current stats and actual empty-inventory labels rather than the old isolated preview/placeholder.

No files were deleted. Existing definition SO assets and prefabs are not modified.
The pre-existing Town storage-scroll edits and font-asset changes are not reverted.

## Executed verification (2026-10-20)

- Unity Editor compilation executed successfully.
- Equipment integration Play Mode: 108 checks, 0 errors, no failure. Includes actual recipe controls/crafting button, ownership rows and raycast accessibility, equip/unequip/replacement, shared wearers, duplicate options, distinct sets, set activation, gauge conversion, overflow/failure atomicity, value-record serialization, scene lifetime, combat selection and equipped HP initialization.
- Existing character/combat regression Play Mode: 10,092 checks, 0 errors, no failure. Includes Ern TripleSlash and all six existing Mayhem actions, movement/jump, spawn, camera and scene entry.
- Existing Town Transform/RectTransform blocks match HEAD. No SO definition assets, prefabs or animation assets changed.
- Scoped `git diff --check` passed. Whole-worktree check still reports the three pre-existing font-asset trailing-whitespace lines (133, 150, 157); these user changes were preserved.
- Player build and prolonged manual gameplay were not performed. Production save/restore and real reward grants are not implemented by this task.
