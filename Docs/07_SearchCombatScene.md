# Search_Combat scene foundation

## Scope and scene references

- `Assets/Scenes/Search_Combat.unity` is a separate scene copied from the latest local Dev_Combat, with a new scene GUID.
- Existing local fileIDs and external Prefab/asset GUIDs are retained. Floor colliders, Grid/Tilemap, standalone player, CombatPlayerEntry, directional camera framing, OrthographicCameraBounds, HUD, GoblinPool and result controls are copied unchanged.
- Search is appended to EditorBuildSettings without changing the existing scenes' order or Town's Dev_Combat destination.
- BackDev uses `Assets/PrototypeAsset/Map/Stage_001_GoblinHamlet/Stage_001_GoblinHamlet_BackGround.png` (single Sprite, PPU 256, 1983 x 793 pixels).
- Only in Search, background XY scale is 6.07556116. It retains the Dev background's 18.82-unit world height and image aspect ratio. World width is approximately 47.06186633.
- CameraBounds remains a disabled, definition-only BoxCollider2D. Its existing center/offset are retained and its transform XY scale is 47.06186633 x 18.82, matching the displayed background. Runtime background-to-bounds auto-resizing is not added.
- Original floor/tile placement is retained, not resized or painted for the new artwork. Final level/ground-art alignment is a separate task.

## Stage data prepared, not executed

`Combat Player Entry` has a small `CombatSceneDefinition` component referring to `Assets/SOData/StageData/Search/Stage_001_GoblinHamlet.asset`.
It holds only a static reference. It does not change `CombatPlayerEntry.SelectedMap`, enemy spawning, completion rules or rewards.

Current MapData fields: stageCode, thumbnail, mapName, description, numeric rank, corruption, mapType, unlock definition, enemies and gold/memory rewards.
The existing asset defines code 001, Search, rank/corruption 1, one MonsterData reference, 100 gold and 100 memory fragments; thumbnail and prerequisite stage are empty and unlockedByDefault is false.
Those values are preserved, not invented or applied by this scene foundation. Enemy counts, waves, placements, objectives and reward issuance still need explicit integration/specifications.

## Dev controls: review only

Spawn/Clear/BackMenu controls and their existing events are retained. No production UI is claimed.
For a later UI split, keep the existing Button references and place only debug controls under a dedicated optional debug group, or extract the debug HUD to a Prefab.
Do not destroy button objects while CombatResultController still owns their references.
The current SceneLoader reloads the active scene, so Search retry reloads Search; CombatResultController still checks the availability of Dev_Combat before retry, and that legacy validation should be revisited with production routing.
Result labels still contain standalone/test wording when no runtime stage selection exists.

## Verification

`Tools > Slayers > Verify Search Combat` (Ctrl+Shift+F5) runs both Search and Dev in Play Mode.
It verifies scene load, actual motor movement, floor collision/ground detection, camera follow and full-viewport confinement, background and stage references, HUD, retained nonempty Tilemap, spawn-button/pool behavior and missing scripts.
Test inputs/grants or stage spawns are not added to production initialization.
Town routing, full character-skill combat regression, final level traversal/art alignment and a standalone Player build remain outside this verification.

Executed in Unity 6000.3.22f1: compilation succeeded; the final two-scene Play Mode run passed 347 checks, zero runtime errors and no test failure.
Dev_Combat's on-disk SHA256 remained identical to the pre-task local snapshot, including the user's existing changes.
While the Tile Palette/Scene view was open, Unity's Tilemap editor reported `Screen position out of view frustum` from GridEditorUtility.ScreenToLocal.
That editor-only issue is reported separately, not suppressed or treated as a runtime scene failure; the Game-view Play Mode checks did not reproduce it.
