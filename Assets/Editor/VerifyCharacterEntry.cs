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
        var statChecks = VerifyTownStats(menu);
        while (statChecks.MoveNext()) yield return statChecks.Current;
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
            Check(controller.SelectedCharacter == mayhem.character && mayhem.button.IsActive() && ern.button.IsActive(), "First character Mayhem selected and both options visible");
            CheckSelected(mayhem.button, true); CheckSelected(ern.button, false);
            yield return new WaitForEndOfFrame();
            Check(mayhem.button.transform.position.y > ern.button.transform.position.y, "Mayhem above Ern in authored character list");
            Check(mayhem.button.transform.parent.GetSiblingIndex() < ern.button.transform.parent.GetSiblingIndex(), "Character hierarchy order matches visible order");
            CheckClickable(mayhem.button); CheckClickable(ern.button);
            Check(mayhem.button.onClick.GetPersistentEventCount() == 0 && ern.button.onClick.GetPersistentEventCount() == 0,
                "Character buttons use only existing Awake-bound selection events");
            Check(mayhem.icon.sprite == mayhem.character.thumbnail && ern.icon.sprite == ern.character.thumbnail, "Existing SO thumbnails");
            var fields = (TownCombatCharacterSelectController.StatField[])Read(controller,"statFields"); Check(fields.Length == 26, "Only existing 26 stat locations");
            foreach (var option in new[] { ern, mayhem })
            {
                Click(option.button);
                Check(controller.SelectedCharacter == option.character, "Character button updates selection");
                Check(TownCharacterSelection.Character == option.character && TownCharacterSelection.Level == 1,
                    "Town session selection matches combat character button");
                Check(((TMP_Text)Read(controller,"characterNameText")).text == option.character.characterName && ((TMP_Text)Read(controller,"levelText")).text == "레벨 1", "SO name and temporary level 1");
                Check(((Image)Read(controller,"fullBodyImage")).sprite == option.character.fullBodyImage, "SO full body image");
                var preview = (CharacterStats)Read(controller,"preview");
                foreach (var stat in fields) Check(stat.text.text == TownCombatCharacterSelectController.FormatStat(stat.stat, preview.GetFinalStat(stat.stat)), "Actual level stat " + stat.stat);
                Check(((TMP_Text[])Read(controller,"equipmentTexts")).All(t => t.text == "없음"), "Empty inventory displays no invented equipment");
            }
            Check(TownCombatCharacterSelectController.FormatStat(StatType.Health, 1) == "1" && TownCombatCharacterSelectController.FormatStat(StatType.CriticalDamage, 200) == "200%", "Numeric / percent formatting");
            nav.OpenQuestDescription();
            Click(ButtonField(controller,"backButton")); Check(stages.SelectedMapData == map, "Back selects first stage");
            CheckSelected(ButtonField(nav,"basicButton"),true); CheckSelected(ButtonField(nav,"questButton"),false);
            Click(ButtonField(controller,"stageConfirm")); Check(controller.SelectedCharacter == mayhem.character, "Reentry defaults to first character Mayhem");
            var chosen = chooseErn ? ern : mayhem; Click(chosen.button); var definition = chosen.character;
            Click(ButtonField(controller,"characterConfirm")); yield return null; yield return null; yield return null;
            Check(SceneManager.GetActiveScene().name == "Dev_Combat", "Dev_Combat loaded");
            var entry = FindFirstObjectByType<CombatPlayerEntry>(); Check(entry.SelectedMap == map && entry.SelectedCharacter == definition, "Selection delivered across scene load");
            Check(TownCharacterSelection.Character == entry.SelectedCharacter && TownCharacterSelection.Level == 1,
                "Town session selection matches consumed combat entry");
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

    private IEnumerator VerifyTownStats(TownMenuController menu)
    {
        var values = Enum.GetValues(typeof(StatType)).Cast<StatType>().ToArray();
        Check(values.Length == 35 && values.Select(v => (int)v).SequenceEqual(Enumerable.Range(0, 36).Where(v => v != 14)),
            "All original serialized stat values preserved; 14 retired");
        var originalNames = ("Health Mana Attack Magic Defense Resistance AttackSpeed MoveSpeed StatusResistance Luck " +
            "Accuracy Evasion Recovery CooldownReduction Stagger CriticalChance CriticalDamage DefensePenetration ResistancePenetration " +
            "FireEnhancement WaterEnhancement EarthEnhancement WindEnhancement LightningEnhancement IceEnhancement LightEnhancement DarkEnhancement " +
            "FireResistance WaterResistance EarthResistance WindResistance LightningResistance IceResistance LightResistance DarkResistance").Split(' ');
        for (int i = 0; i < values.Length; i++)
        {
            Check(values[i].ToString() == originalNames[i], "Preserved serialized identity " + originalNames[i]);
            Check(CharacterStatPanelView.FormatStat(values[i], 12.5f) == (i < 4 ? "12.5" : "12.5%"), "Independent numeric / percent rule " + values[i]);
        }
        var expected = new[] {
            (StatType.Health, "BasicStat/Health"), (StatType.Mana, "BasicStat/Mana"),
            (StatType.Attack, "BasicStat/AttackPower"), (StatType.Magic, "BasicStat/MagicPower"),
            (StatType.Defense, "BasicStat/Defense"), (StatType.Resistance, "BasicStat/Resist"),
            (StatType.DefensePenetration, "BasicStat/DefensePen"), (StatType.ResistancePenetration, "BasicStat/ResistPen"),
            (StatType.Stagger, "SubStat/StaggerAttack"), (StatType.CriticalChance, "SubStat/CriticalRate"),
            (StatType.CriticalDamage, "SubStat/CriticalPower"), (StatType.AttackSpeed, "SubStat/AttackSpeed"),
            (StatType.MoveSpeed, "SubStat/MovementSpeed"), (StatType.Accuracy, "SubStat/Accurancy"),
            (StatType.Evasion, "SubStat/Evasion"), (StatType.StatusResistance, "SubStat/StatusResist"),
            (StatType.Recovery, "SubStat/Recovery"), (StatType.CooldownReduction, "SubStat/Cooldown"),
            (StatType.Luck, "SubStat/Luck"),
            (StatType.FireEnhancement, "ElementPowerStat/Fire"), (StatType.WaterEnhancement, "ElementPowerStat/Water"),
            (StatType.EarthEnhancement, "ElementPowerStat/Ground"), (StatType.WindEnhancement, "ElementPowerStat/Wind"),
            (StatType.LightningEnhancement, "ElementPowerStat/Thunder"), (StatType.IceEnhancement, "ElementPowerStat/Ice"),
            (StatType.LightEnhancement, "ElementPowerStat/Light"), (StatType.DarkEnhancement, "ElementPowerStat/Dark"),
            (StatType.FireResistance, "ElementResistStat/Fire"), (StatType.WaterResistance, "ElementResistStat/Water"),
            (StatType.EarthResistance, "ElementResistStat/Ground"), (StatType.WindResistance, "ElementResistStat/Wind"),
            (StatType.LightningResistance, "ElementResistStat/Thunder"), (StatType.IceResistance, "ElementResistStat/Ice"),
            (StatType.LightResistance, "ElementResistStat/Light"), (StatType.DarkResistance, "ElementResistStat/Dark")
        };
        menu.OpenCharacter();
        var navigation = FindFirstObjectByType<TownCharacterNavigationController>();
        var controller = FindFirstObjectByType<TownCharacterStatsController>();
        var detail = (CharacterStatPanelView)Read(controller, "detailView");
        var overview = (CharacterStatPanelView)Read(controller, "overviewView");
        var townMayhem = (Button)Read(Read(controller, "mayhem"), "button");
        var townErn = (Button)Read(Read(controller, "ern"), "button");
        yield return new WaitForEndOfFrame();
        Check(townMayhem.IsActive() && townErn.IsActive() && townMayhem.transform.position.y > townErn.transform.position.y,
            "Character menu list displays Mayhem above Ern");
        CheckClickable(townMayhem); CheckClickable(townErn);
        CheckSelected(townMayhem, true); CheckSelected(townErn, false);
        var fields = (CharacterStatPanelView.StatField[])Read(detail, "statFields");
        Check(fields.Length == 35 && fields.Select(f => f.stat).Distinct().Count() == 35, "All 35 detail fields uniquely wired");
        foreach (var binding in expected)
        {
            var text = detail.transform.Find(binding.Item2 + "/Text").GetComponent<TMP_Text>();
            Check(fields.Single(f => f.stat == binding.Item1).text == text, "Explicit detail TMP binding " + binding.Item1);
            var icons = text.transform.parent.GetComponentsInChildren<Image>(true).Where(i => i.sprite != null).ToArray();
            Check(icons.Length > 0, "Stat icon reference present " + binding.Item1);
            Debug.Log("[StatDetailTest] Icon " + binding.Item1 + "=" + string.Join(",", icons.Select(i => i.sprite.name)));
        }
        var gauge = (TMP_Text)Read(detail, "staggerGaugeText");
        Check(gauge == detail.transform.Find("SubStat/StaggerGauge/Text").GetComponent<TMP_Text>(), "Derived gauge TMP binding");
        CheckOrder(detail.transform, expected.Take(8).Select(e => e.Item2).ToArray());
        CheckOrder(detail.transform, new[] { "SubStat/StaggerAttack", "SubStat/StaggerGauge", "SubStat/CriticalRate", "SubStat/CriticalPower",
            "SubStat/AttackSpeed", "SubStat/MovementSpeed", "SubStat/Accurancy", "SubStat/Evasion",
            "SubStat/StatusResist", "SubStat/Recovery", "SubStat/Cooldown", "SubStat/Luck" });
        CheckOrder(detail.transform, expected.Skip(19).Take(8).Select(e => e.Item2).ToArray());
        CheckOrder(detail.transform, expected.Skip(27).Select(e => e.Item2).ToArray());
        var title = (TMP_Text)Read(Read(navigation, "titleController"), "titleText");
        var select = FindObjectsByType<TownCombatCharacterSelectController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        foreach (var optionName in new[] { "ern", "mayhem" })
        {
            var option = (TownCombatCharacterSelectController.CharacterOption)Read(select, optionName);
            var townOption = Read(controller, optionName);
            Check((PlayableCharacterData)Read(townOption, "character") == option.character, "Town list reuses existing character SO");
            Check(((Image)Read(townOption, "icon")).sprite == option.character.thumbnail, "Town list uses character thumbnail");
            Click((Button)Read(townOption, "button"));
            Check(TownCharacterSelection.Character == option.character && TownCharacterSelection.Level == 1, "Town list click updates shared selection");
            CheckSelected(optionName == "ern" ? townErn : townMayhem, true);
            CheckSelected(optionName == "ern" ? townMayhem : townErn, false);
            Check(((TMP_Text)Read(controller, "characterNameText")).text == option.character.characterName, "Town name follows selection");
            Check(((Image)Read(controller, "fullBodyImage")).sprite == option.character.fullBodyImage, "Town illustration follows selection");
            var stats = controller.CurrentStats;
            Check(stats.Data == option.character.statsData && stats.Level == 1, "Town stats use selected SO / level");
            yield return new WaitForEndOfFrame();
            CheckClickable(ButtonField(controller, "openDetailButton"));
            Click(ButtonField(controller, "openDetailButton"));
            yield return new WaitForEndOfFrame();
            CheckClickable(ButtonField(controller, "closeDetailButton"));
            Check(detail.gameObject.activeInHierarchy && title.text == "기본 정보", "Details open under Basic Info title");
            VerifyView(detail, stats); VerifyView(overview, stats);
            var modifier = new StatModifier(StatType.Health, StatModifierType.Flat, 10f, "stat-verification");
            float baseHealth = stats.MaxHealth;
            stats.AddModifier(modifier);
            Check(Mathf.Approximately(stats.FinalMaxHealth, baseHealth + 10f), "Existing flat modifier calculation");
            VerifyView(detail, stats); VerifyView(overview, stats);
            var percent = new StatModifier(StatType.Health, StatModifierType.Percent, 20f, "stat-verification");
            stats.AddModifier(percent);
            Check(Mathf.Approximately(stats.FinalMaxHealth, (baseHealth + 10f) * 1.2f), "Existing flat then percent modifier calculation");
            VerifyView(detail, stats); VerifyView(overview, stats);
            Click(ButtonField(controller, "closeDetailButton"));
            Check(!detail.gameObject.activeSelf, "Details close");
            stats.RemoveModifier(modifier);
            stats.RemoveModifier(percent);
            Click(ButtonField(controller, "openDetailButton"));
            Check(Mathf.Approximately(stats.FinalMaxHealth, baseHealth), "Modifier removal restores definition");
            VerifyView(detail, stats);
            Click(ButtonField(navigation, "equipButton"));
            Check(!detail.gameObject.activeInHierarchy && title.text == "장비", "Equipment tab hides details");
            Click(ButtonField(navigation, "skillButton")); Check(title.text == "스킬", "Skill tab title");
            Click(ButtonField(navigation, "statButton"));
            Check(title.text == "기본 정보" && !detail.gameObject.activeSelf, "Basic info returns with details closed");
            menu.OpenStorage(); menu.OpenCharacter();
            Check(TownCharacterSelection.Character == option.character, "Town reentry preserves character selection");
            VerifyView(overview, controller.CurrentStats);
        }
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var component in root.GetComponentsInChildren<Component>(true))
                Check(component != null, "No missing scene scripts");
        foreach (var component in new UnityEngine.Object[] { navigation, controller, detail, overview, select })
        {
            var serialized = new SerializedObject(component);
            var property = serialized.GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                    Check(property.objectReferenceValue != null || property.objectReferenceInstanceIDValue == 0,
                        "No broken serialized reference " + component.name + "/" + property.propertyPath);
        }
        Debug.Log("[StatDetailTest] 35 stats + derived gauge, authored ordering/icons/TMP bindings, formatting, modifiers, reopen, character selection and three tabs verified.");
        menu.OpenBattle();
    }

    private void CheckClickable(Button button)
    {
        var point = RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)button.transform).TransformPoint(((RectTransform)button.transform).rect.center));
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
        Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
            "Button is not blocked by another UI graphic: " + button.name + "; first hit=" + (hits.Count > 0 ? hits[0].gameObject.name : "none"));
    }

    private void VerifyView(CharacterStatPanelView view, CharacterStats stats)
    {
        foreach (var field in (CharacterStatPanelView.StatField[])Read(view, "statFields"))
            Check(field.text != null && field.text.text == CharacterStatPanelView.FormatStat(field.stat, stats.GetFinalStat(field.stat)),
                "Fresh final stat display " + field.stat);
        var gauge = (TMP_Text)Read(view, "staggerGaugeText");
        if (gauge != null)
            Check(gauge.text == CharacterStatPanelView.FormatNumber(stats.FinalMaxHealth * stats.Data.staggerGaugeHealthPercent / 100f)
                && !gauge.text.Contains("%"), "Gauge uses final HP-derived numeric value");
    }

    private void CheckOrder(Transform root, string[] paths)
    {
        var ordered = paths.Select(p => root.Find(p)).OrderByDescending(t => Mathf.Round(t.position.y * 100f))
            .ThenBy(t => t.position.x).Select(t => t.name).ToArray();
        Check(ordered.SequenceEqual(paths.Select(p => p.Substring(p.LastIndexOf('/') + 1))), "Authored row-major stat order " + paths[0]);
    }
}
