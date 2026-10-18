using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public static class VerifyCharacterEntry
{
    [MenuItem("Tools/Slayers/Verify Character Entry %#F8")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Town.unity");
        EditorApplication.playModeStateChanged += Entered; EditorApplication.isPlaying = true;
    }
    private static void Entered(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= Entered;
        var root = new GameObject("Temporary Character Entry Verification"); UnityEngine.Object.DontDestroyOnLoad(root); root.AddComponent<CharacterEntryVerification>();
    }
}

public class CharacterEntryVerification : MonoBehaviour
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private int checks, errors;
    private object Read(object target, string field) => target.GetType().GetField(field, Flags).GetValue(target);
    private void Input(PlayerInputReader reader, string field, object value) => typeof(PlayerInputReader).GetField("<" + field + ">k__BackingField", Flags).SetValue(reader, value);
    private void Check(bool result, string label) { checks++; if (!result) throw new Exception(label); }
    private void Click(Button button) => ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
    private Button ButtonField(object target, string field) => (Button)Read(target, field);
    private void CheckSelected(Button button, bool selected)
    {
        var visual = button.GetComponent<MenuButtonVisual>();
        Check(visual != null && (bool)Read(visual, "isSelected") == selected, "Visible selection matches button " + button.name);
    }
    private IEnumerator Start()
    {
        Application.logMessageReceived += Log;
        var body = Body(); bool failed = false;
        while (true)
        {
            object next;
            try { if (!body.MoveNext()) break; next = body.Current; }
            catch (Exception error) { failed = true; Debug.LogError("[CharacterEntryTest] FAIL " + error); break; }
            yield return next;
        }
        Application.logMessageReceived -= Log;
        Debug.Log($"[CharacterEntryTest] RESULT checks={checks}, errors={errors}, failed={failed}");
        EditorApplication.delayCall += () => EditorApplication.isPlaying = false;
    }
    private void Log(string message, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private IEnumerator Body()
    {
        yield return null; yield return null;
        Check(!CombatEntrySelection.TrySet(null, null, null, 1), "Invalid selection rejected");
        var menu = FindFirstObjectByType<TownMenuController>();
        for (int repeat = 0; repeat < 2; repeat++)
        {
            menu.OpenShop();
            var shop = FindFirstObjectByType<TownShopNavigationController>();
            CheckSelected(ButtonField(shop,"generalExchangeButton"),true);
            CheckSelected(ButtonField(shop,"blackMarketButton"),false);
            CheckSelected(ButtonField(shop,"buyButton"),true);
            shop.OpenBlackSmith(); CheckSelected(ButtonField(shop,"craftButton"),true);
            shop.OpenEnhance(); CheckSelected(ButtonField(shop,"enhanceButton"),true); CheckSelected(ButtonField(shop,"craftButton"),false);
            menu.OpenStorage();
            var storage = FindFirstObjectByType<TownStorageNavigationController>();
            CheckSelected(ButtonField(storage,"allButton"),true);
            storage.OpenQuest(); CheckSelected(ButtonField(storage,"questButton"),true); CheckSelected(ButtonField(storage,"allButton"),false);
            menu.OpenCharacter();
            var character = FindFirstObjectByType<TownCharacterNavigationController>();
            CheckSelected(ButtonField(character,"statButton"),true);
            character.OpenSkill(); CheckSelected(ButtonField(character,"skillButton"),true); CheckSelected(ButtonField(character,"statButton"),false);
            menu.OpenResearch(); CheckSelected(ButtonField(FindFirstObjectByType<TownResearchNavigationController>(),"skillButton"),true);
            menu.OpenBattle();
            var battle = FindFirstObjectByType<TownBattleNavigationController>();
            CheckSelected(ButtonField(battle,"searchButton"),true);
            battle.SelectRaid(); battle.OpenBattleSelect(); CheckSelected(ButtonField(battle,"searchButton"),true); CheckSelected(ButtonField(battle,"raidButton"),false);
        }
        foreach (bool chooseErn in new[] { true, false })
        {
            if (SceneManager.GetActiveScene().name != "Town") { SceneManager.LoadScene("Town"); yield return null; yield return null; }
            var controller = FindFirstObjectByType<TownCombatCharacterSelectController>();
            var nav = FindFirstObjectByType<TownBattleNavigationController>(); nav.OpenStageSelect();
            CheckSelected(ButtonField(nav,"basicButton"),true);
            var stages = (TownStageSelectController)Read(controller, "stages");
            for (int rebuild = 0; rebuild < 3; rebuild++)
            {
                stages.Refresh(); Canvas.ForceUpdateCanvases(); yield return null;
                var content = (RectTransform)Read(stages,"rankContent");
                var group = content.GetComponent<HorizontalLayoutGroup>();
                var placeholders = (GameObject[])Read(stages,"rankPlaceholders");
                var rows = content.Cast<Transform>().Where(t => t.gameObject.activeSelf && t.name.StartsWith("Rank_")).ToArray();
                Check(rows.Length > 0, "Generated rank rows exist");
                var manager = (StageManager)Read(stages,"stageManager");
                Check(stages.SelectedRank == manager.GetAvailableRanks(stages.SelectedMapType)[0], "First rank selected");
                Check(stages.SelectedMapData == manager.GetAvailableStages(stages.SelectedMapType, stages.SelectedRank.Value)[0], "First stage selected");
                foreach (DictionaryEntry row in (IDictionary)Read(stages,"rankRows"))
                    CheckSelected((Button)Read(row.Value,"Button"), (int)row.Key == stages.SelectedRank);
                foreach (DictionaryEntry row in (IDictionary)Read(stages,"stageRows"))
                    CheckSelected((Button)Read(row.Value,"Button"), (MapData)row.Key == stages.SelectedMapData);
                int slots = Mathf.Max(rows.Length, placeholders.Length);
                float expectedWidth = (content.rect.width - group.padding.horizontal - group.spacing * (slots - 1)) / slots;
                foreach (var row in rows)
                {
                    Check(Mathf.Abs(((RectTransform)row).rect.width - expectedWidth) < 1f && expectedWidth > 10f, "Rank width preserves authored slot instead of hidden template width");
                    Check(Mathf.Abs(((RectTransform)row).anchoredPosition.x - group.padding.left) < 1f || row != rows[0], "First rank remains at left edge");
                }
            }
            Check(stages.SelectedMapData != null, "Real unlocked stage available"); var map = stages.SelectedMapData;
            Click(ButtonField(controller,"stageConfirm"));
            var mayhem = (TownCombatCharacterSelectController.CharacterOption)Read(controller, "mayhem");
            var ern = (TownCombatCharacterSelectController.CharacterOption)Read(controller, "ern");
            Check(controller.SelectedCharacter == ern.character && mayhem.button.IsActive() && ern.button.IsActive(), "First character Ern selected and both options visible");
            CheckSelected(ern.button, true); CheckSelected(mayhem.button, false);
            Check(mayhem.icon.sprite == mayhem.character.thumbnail && ern.icon.sprite == ern.character.thumbnail, "Existing SO thumbnails");
            var fields = (TownCombatCharacterSelectController.StatField[])Read(controller,"statFields"); Check(fields.Length == 26, "Only existing 26 stat locations");
            foreach (var option in new[] { ern, mayhem })
            {
                Click(option.button);
                Check(controller.SelectedCharacter == option.character, "Character button updates selection");
                Check(((TMP_Text)Read(controller,"characterNameText")).text == option.character.characterName && ((TMP_Text)Read(controller,"levelText")).text == "레벨 1", "SO name and temporary level 1");
                Check(((Image)Read(controller,"fullBodyImage")).sprite == option.character.fullBodyImage, "SO full body image");
                var preview = (CharacterStats)Read(controller,"preview");
                foreach (var stat in fields) Check(stat.text.text == TownCombatCharacterSelectController.FormatStat(stat.stat, preview.GetFinalStat(stat.stat)), "Actual level stat " + stat.stat);
                Check(((TMP_Text[])Read(controller,"equipmentTexts")).All(t => t.text == "미등록"), "No invented equipment");
            }
            Check(TownCombatCharacterSelectController.FormatStat(StatType.Awakening, 1) == "1" && TownCombatCharacterSelectController.FormatStat(StatType.CriticalDamage, 200) == "200%", "Numeric / percent formatting");
            nav.OpenQuestDescription();
            Click(ButtonField(controller,"backButton")); Check(stages.SelectedMapData == map, "Back selects first stage");
            CheckSelected(ButtonField(nav,"basicButton"),true); CheckSelected(ButtonField(nav,"questButton"),false);
            Click(ButtonField(controller,"stageConfirm")); Check(controller.SelectedCharacter == ern.character, "Reentry defaults to first character Ern");
            var chosen = chooseErn ? ern : mayhem; Click(chosen.button); var definition = chosen.character;
            Click(ButtonField(controller,"characterConfirm")); yield return null; yield return null; yield return null;
            Check(SceneManager.GetActiveScene().name == "Dev_Combat", "Dev_Combat loaded");
            var entry = FindFirstObjectByType<CombatPlayerEntry>(); Check(entry.SelectedMap == map && entry.SelectedCharacter == definition, "Selection delivered across scene load");
            Check(((Image)Read(entry,"portraitImage")).sprite == definition.thumbnail && ((Image)Read(entry,"portraitImage")).enabled, "HUD portrait uses selected CharacterData thumbnail");
            var player = entry.ActivePlayer; var stats = player.GetComponent<CharacterStats>();
            Check(stats.Data == definition.statsData && stats.Level == 1, "Correct stats and level at spawn");
            yield return new WaitForEndOfFrame();
            Vector3 framed = ((Camera)Read(entry,"combatCamera")).WorldToViewportPoint(player.transform.position);
            Check(Mathf.Abs(framed.x - .19f) < .005f && Mathf.Abs(framed.y - .15f) < .005f, "Selected character matches reference viewport framing: " + framed);
            Check(FindObjectsByType<CharacterMotor2D>(FindObjectsSortMode.None).Length == 1 && PlayerInput.all.Count == 1, "Exactly one active player / PlayerInput");
            Check((player.GetComponent<MayhemMechanism>() == null) == chooseErn, "Correct character mechanism");
            Check(player.GetComponentInChildren<Animator>().runtimeAnimatorController.name == (chooseErn ? "Prototype_Ern" : "MayhemAnimator"), "Correct existing Animator");
            var pool = FindFirstObjectByType<GoblinPool>(); Check((Transform)Read(pool,"player") == player.transform, "Goblin pool targets selected player");
            Check((Health)Read(FindFirstObjectByType<PlayerHealthUI>(),"health") == player.GetComponent<Health>() && (PlayerMana)Read(FindFirstObjectByType<PlayerManaUI>(),"playerMana") == player.GetComponent<PlayerMana>(), "Combat HUD rebound");
            var spawnButton = FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == "SpawnButton"); Click(spawnButton);
            var goblin = FindFirstObjectByType<GoblinEnemy>(); Check(goblin != null, "SpawnButton works with selected character"); goblin.Despawn();
            var motor = player.GetComponent<CharacterMotor2D>(); var reader = player.GetComponent<PlayerInputReader>();
            float until = Time.time + 3; while (!motor.IsGrounded && Time.time < until) yield return null; Check(motor.IsGrounded,"Grounded");
            var camera = (Camera)Read(entry,"combatCamera"); var offset = camera.transform.position - player.transform.position; float startX = player.transform.position.x;
            Input(reader,"MoveInput",Vector2.right); until = Time.time + .15f; while (Time.time < until) yield return null; Input(reader,"MoveInput",Vector2.zero);
            Check(player.transform.position.x > startX + .1f, "Movement works"); yield return new WaitForEndOfFrame();
            Check(Vector3.Distance(camera.transform.position - player.transform.position, offset) < .03f, "Camera follows selected player");
            Input(reader,"JumpPressed",true); yield return null; yield return null; Check(player.GetComponent<Rigidbody2D>().linearVelocity.y > 0, "Jump works");
            until = Time.time + 3; yield return null; while (!motor.IsGrounded && Time.time < until) yield return null;
            Check(motor.IsGrounded,"Landed before skill");
            if (chooseErn)
            {
                float mana = player.GetComponent<PlayerMana>().CurrentMana; Input(reader,"TestSkillPressed",true); yield return null; yield return null;
                Check(player.GetComponent<PlayerMana>().CurrentMana < mana, "Ern TripleSlash consumes existing mana cost");
                Check(FindObjectsByType<AttackHitboxController>(FindObjectsSortMode.None).Any(c => c.name.Contains("TripleSlash")), "Ern existing TripleSlash effect spawned");
                Debug.Log("[CharacterEntryTest] Ern Town -> combat, stats/UI, single player, movement/jump/camera, Goblin spawn and TripleSlash verified.");
            }
            else
            {
                foreach (var key in new[] { ("BasicAttackPressed",1), ("SecondaryAttackPressed",1), ("BloodSpearPressed",2), ("BloodAxePressed",3), ("BloodRampagePressed",7), ("TestSkillPressed",3) })
                {
                    Input(reader,key.Item1,true); yield return null; yield return null; var mechanism = player.GetComponent<MayhemMechanism>(); Check(mechanism.IsActing, "Mayhem action starts " + key.Item1);
                    var seen = new HashSet<int>(); until = Time.time + 8;
                    do
                    {
                        foreach (var fx in FindObjectsByType<AttackHitboxController>(FindObjectsSortMode.None)) if (fx.name.Contains("Mayhem")) seen.Add(fx.GetInstanceID());
                        Check(Time.time < until, "Action timeout"); yield return null;
                    } while (mechanism.IsActing);
                    float tail = Time.time + .6f; while (Time.time < tail) { foreach (var fx in FindObjectsByType<AttackHitboxController>(FindObjectsSortMode.None)) if (fx.name.Contains("Mayhem")) seen.Add(fx.GetInstanceID()); yield return null; }
                    Check(seen.Count == key.Item2, "Mayhem effect count " + key.Item1 + "=" + seen.Count);
                    Check(motor.CurrentMovementControl == CharacterMotor2D.MovementControl.Free,"Movement restored");
                }
                Debug.Log("[CharacterEntryTest] Mayhem Town -> combat and all six existing actions / effect counts verified.");
            }
        }
        SceneManager.LoadScene("Dev_Combat"); yield return null; yield return null;
        var standalone = FindFirstObjectByType<CombatPlayerEntry>();
        Check(standalone.SelectedCharacter == null && standalone.ActivePlayer.name == "Playerable", "Direct Dev_Combat retains authored player after consumed handoff");
        Check(((Image)Read(standalone,"portraitImage")).sprite == ((PlayableCharacterData)Read(standalone,"standaloneCharacter")).thumbnail, "Standalone HUD uses authored Mayhem CharacterData thumbnail");
        Check(FindObjectsByType<CharacterMotor2D>(FindObjectsSortMode.None).Length == 1, "Standalone has one player");
        yield return new WaitForEndOfFrame();
        Vector3 standaloneFraming = ((Camera)Read(standalone,"combatCamera")).WorldToViewportPoint(standalone.ActivePlayer.transform.position);
        Check(Mathf.Abs(standaloneFraming.x - .19f) < .005f && Mathf.Abs(standaloneFraming.y - .15f) < .005f, "Standalone matches reference viewport framing");
        Click(FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == "SpawnButton")); Check(FindFirstObjectByType<GoblinEnemy>() != null, "Standalone SpawnButton works");
        Click(FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == "BackMenuButton"));
        yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "Town", "BackMenuButton returns to Town");
        Check(FindFirstObjectByType<CharacterMotor2D>() == null && FindFirstObjectByType<GoblinEnemy>() == null,
            "Returning to Town unloads combat player and goblin");
    }
}
