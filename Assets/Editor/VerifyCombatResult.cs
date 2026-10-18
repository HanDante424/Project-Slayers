using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class VerifyCombatResult
{
    [MenuItem("Tools/Slayers/Preview Combat Result %#F10")]
    public static void Preview()
    {
        if (EditorApplication.isPlaying)
            UnityEngine.Object.FindFirstObjectByType<CombatResultController>()?.CompleteForTesting();
    }

    [MenuItem("Tools/Slayers/Verify Combat Result %#F9")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Dev_Combat.unity");
        EditorApplication.playModeStateChanged += Entered;
        EditorApplication.isPlaying = true;
    }
    private static void Entered(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= Entered;
        var root = new GameObject("Temporary Combat Result Verification");
        UnityEngine.Object.DontDestroyOnLoad(root);
        root.AddComponent<CombatResultVerification>();
    }
}

public sealed class CombatResultVerification : MonoBehaviour
{
    private int checks, errors;
    private object Read(object target, string field) => target.GetType().GetField(field,
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    private void Check(bool result, string label)
    { checks++; if (!result) throw new Exception(label); }
    private void Click(Button button) => ExecuteEvents.Execute(button.gameObject,
        new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
    private T Find<T>() where T : UnityEngine.Object => FindFirstObjectByType<T>();
    private TMP_Text Text(CombatResultView view, string field) => (TMP_Text)Read(view, field);
    private void Log(string message, string trace, LogType type)
    { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }

    private IEnumerator Start()
    {
        Application.logMessageReceived += Log;
        var body = Body(); bool failed = false;
        while (true)
        {
            object next;
            try { if (!body.MoveNext()) break; next = body.Current; }
            catch (Exception error) { failed = true; Debug.LogError("[CombatResultTest] FAIL " + error); break; }
            yield return next;
        }
        Application.logMessageReceived -= Log;
        Debug.Log($"[CombatResultTest] RESULT checks={checks}, errors={errors}, failed={failed}");
        Time.timeScale = 1f;
        EditorApplication.delayCall += () => EditorApplication.isPlaying = false;
    }

    private IEnumerator Body()
    {
        yield return null; yield return null;
        Check(CombatRecord.FormatTime(0) == "00:00" && CombatRecord.FormatTime(3601) == "60:01" &&
            CombatRecord.FormatTime(600001) == "10000:01", "Time has no hour/minute wrapping");
        var controller = Find<CombatResultController>();
        var view = Find<CombatResultView>();
        var entry = Find<CombatPlayerEntry>();
        Check(!view.IsVisible && controller.Result == null && controller.Record.IsRecording, "Initial result hidden / recorder running");
        var scroll = (ScrollRect)Read(view,"itemScroll");
        Check(scroll.content.Cast<Transform>().All(t => !t.gameObject.activeSelf), "Authored fake items hidden");
        var player = entry.ActivePlayer;
        var playerHealth = player.GetComponent<Health>();
        Click((Button)Read(controller,"spawnButton"));
        var goblin = Find<GoblinEnemy>(); Check(goblin != null,"Existing SpawnButton creates goblin");
        goblin.enabled = false; // Isolate deterministic hit accounting from AI in this test only.
        var health = goblin.GetComponent<Health>();
        int enemyHP = health.CurrentHealth;
        var receiver = goblin.GetComponent<HitReceiver>();
        receiver.ReceiveHit(new HitData { damage = 7, source = player });
        Check(health.CurrentHealth == enemyHP - 7 && controller.Record.DamageDealt == 7, "Resolved receiver HP loss counted once");
        player.GetComponent<PlayerHitReceiver>().ReceiveHit(new HitData { damage = 3, source = goblin.gameObject });
        Check(controller.Record.DamageReceived == 3,"Actual incoming damage recorded");
        health.ResetHealth(); playerHealth.ResetHealth();
        Check(controller.Record.DamageDealt == 7 && controller.Record.DamageReceived == 3,"Recovery does not subtract/add damage");
        goblin.Despawn(); Check(controller.Record.Kills == 0,"Despawn is not a kill");
        var pool = Find<GoblinPool>(); pool.SpawnGoblin();
        goblin = Find<GoblinEnemy>(); goblin.enabled = false;
        health = goblin.GetComponent<Health>(); receiver = goblin.GetComponent<HitReceiver>();
        int lifeHP = health.CurrentHealth;
        receiver.ReceiveHit(new HitData { damage = lifeHP + 100, source = player });
        receiver.ReceiveHit(new HitData { damage = lifeHP + 100, source = player });
        Check(controller.Record.Kills == 1 && controller.Record.DamageDealt == 7 + lifeHP,
            "Overkill counts only HP lost; dead targets do not duplicate kills/damage");
        Check(controller.Result == null && Time.timeScale == 1f,"Enemy death does not invent a stage completion rule");
        goblin.Despawn(); pool.SpawnGoblin(); goblin = Find<GoblinEnemy>(); goblin.enabled = false;
        health = goblin.GetComponent<Health>();
        int nextLifeHP = health.CurrentHealth;
        goblin.GetComponent<HitReceiver>().ReceiveHit(new HitData { damage = nextLifeHP, source = player });
        Check(controller.Record.Kills == 2 && controller.Record.DamageDealt == 7 + lifeHP + nextLifeHP,
            "Reused pooled actor counts as a new life, exactly once");
        goblin.Despawn(); pool.SpawnGoblin(); goblin = Find<GoblinEnemy>(); goblin.enabled = false;
        health = goblin.GetComponent<Health>();
        yield return new WaitForSeconds(.1f);
        var inputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        Check(inputModule != null && inputModule.leftClick.action.enabled,"UI click input enabled before result");
        Click((Button)Read(controller,"testClearButton"));
        var result = controller.Result;
        Check(result != null && result.IsTestClear && view.IsVisible && Time.timeScale == 0f,"Clear test button opens test result and pauses combat");
        Check(!controller.Record.IsRecording && result.Kills == 2 && result.DamageDealt == 7 + lifeHP + nextLifeHP &&
            result.DamageReceived == 3,"Immutable result captures actual combat totals");
        Check(Text(view,"stageDescription").text.Contains("미선택") && Text(view,"stageName").text.Contains("없음"),"Standalone has no fabricated stage identity");
        Check(Text(view,"kills").text == "2" && Text(view,"damageDealt").text == result.DamageDealt.ToString() &&
            Text(view,"damageReceived").text == "3", "Existing result TMP fields show actual totals");
        Check(Text(view,"gold").text.Contains("미지급") && Text(view,"memoryFragment").text.Contains("미지급") &&
            Text(view,"rewardTitle").text.Contains("미구현"), "No fake reward receipt");
        int hpBefore = health.CurrentHealth, playerBefore = playerHealth.CurrentHealth;
        health.TakeDamage(50, player); playerHealth.TakeDamage(50, goblin.gameObject);
        Check(health.CurrentHealth == hpBefore && playerHealth.CurrentHealth == playerBefore,"Direct damage blocked after result");
        Check(!player.GetComponent<PlayerActionController>().enabled && !player.GetComponent<CharacterMotor2D>().enabled &&
            !player.GetComponent<PlayerInputReader>().enabled,"Player combat input and movement disabled");
        Check(!pool.enabled && !((Button)Read(controller,"spawnButton")).enabled,"Spawn controls gated after result");
        Check(inputModule.enabled && inputModule.leftClick.action.enabled && inputModule.point.action.enabled,
            "Result pause leaves UI input actions enabled independently of player input");
        controller.CompleteForTesting(); controller.CompleteStage();
        Check(ReferenceEquals(result,controller.Result),"Duplicate completion has no effect");
        double elapsed = controller.Record.ElapsedSeconds;
        yield return new WaitForSecondsRealtime(.1f);
        Check(controller.Record.ElapsedSeconds == elapsed,"Timer and totals stop at completion");
        var itemGuid = AssetDatabase.FindAssets("t:ItemData",new[] {"Assets/SOData"}).FirstOrDefault();
        if (itemGuid != null)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(itemGuid));
            for (int i=0;i<30;i++) view.AddGrantedItem(item,3);
            Canvas.ForceUpdateCanvases();
            var rows = scroll.content.GetComponentsInChildren<CombatResultItemView>();
            Check(rows.Length == 30,"Dynamic slots clone the existing item template");
            Check(rows.All(row => ((Image)Read(row,"icon")).sprite == item.thumbnail &&
                ((TMP_Text)Read(row,"label")).text == item.itemName + "\n×3"),"Cloned item references and counts retained");
            Check(scroll.content.rect.height > scroll.viewport.rect.height && scroll.verticalScrollbar != null,"Existing layout produces scrollable content");
            scroll.verticalNormalizedPosition = 0f; Canvas.ForceUpdateCanvases();
            Check(scroll.verticalNormalizedPosition < .01f,"Existing ScrollRect can move to bottom while combat is paused");
            view.Show(result); yield return null;
            Check(scroll.content.Cast<Transform>().All(t=>!t.gameObject.activeSelf),"Re-show resets item presentation without granting anything");
        }
        else Debug.LogWarning("[CombatResultTest] No ItemData fixture: dynamic item view not tested.");
        Click(view.RetryButton); yield return null; yield return null; yield return null;
        controller = Find<CombatResultController>(); entry = Find<CombatPlayerEntry>(); view = Find<CombatResultView>();
        Check(SceneManager.GetActiveScene().name == "Dev_Combat" && Time.timeScale == 1f && !view.IsVisible &&
            controller.Result == null && controller.Record.DamageDealt == 0 && controller.Record.Kills == 0,"Retry reloads and clears record/UI/pause");
        Check(entry.SelectedMap == null && entry.SelectedCharacter == null && entry.ActivePlayer.name == "Playerable","Standalone retry retains authored player");
        entry.ActivePlayer.GetComponent<Health>().TakeDamage(1);
        Check(controller.Record.DamageReceived == 1,"No stale static event subscriptions after retry");
        controller.CompleteForTesting(); Click(view.TownButton); yield return null; yield return null;
        Check(SceneManager.GetActiveScene().name == "Town" && Time.timeScale == 1f && Find<CharacterMotor2D>() == null,
            "Town return restores time and unloads combat actors");
        foreach (bool ern in new[] {true,false})
        {
            Find<TownBattleNavigationController>().OpenStageSelect();
            var select = Find<TownCombatCharacterSelectController>(); select.OpenSelectedStage();
            if (ern) select.SelectErn(); else select.SelectMayhem();
            var character = select.SelectedCharacter; var map = select.SelectedMap;
            Check(map != null && character != null,"Real Town selection available");
            select.EnterCombat(); yield return null; yield return null; yield return null;
            controller = Find<CombatResultController>(); view = Find<CombatResultView>(); entry = Find<CombatPlayerEntry>();
            Check(entry.SelectedMap == map && entry.SelectedCharacter == character,"Town selection reaches combat");
            controller.CompleteStage(); var selectedResult = controller.Result;
            Check(!selectedResult.IsTestClear && selectedResult.Stage == map && Text(view,"stageName").text == map.mapName &&
                Text(view,"stageDescription").text.Contains(map.rank + "등급") &&
                Text(view,"stageDescription").text.Contains("침식도 " + map.corruption),"Completion API displays selected MapData without inventing an objective");
            Click(view.RetryButton); yield return null; yield return null; yield return null;
            entry = Find<CombatPlayerEntry>(); controller = Find<CombatResultController>(); view = Find<CombatResultView>();
            Check(entry.SelectedMap == map && entry.SelectedCharacter == character && entry.ActivePlayer.GetComponent<CharacterStats>().Level == 1,
                "Retry preserves selected map, character and level " + character.characterName);
            Check(controller.Result == null && controller.Record.Kills == 0 && controller.Record.DamageReceived == 0 && Time.timeScale == 1f,
                "Selected-character retry starts a fresh record");
            controller.CompleteForTesting(); controller.enabled = false;
            Check(Time.timeScale == 1f && entry.ActivePlayer.GetComponent<Health>().DamageEnabled &&
                entry.ActivePlayer.GetComponent<CharacterMotor2D>().enabled,"Forced controller disable restores owned pause and permissions");
            SceneManager.LoadScene("Town"); yield return null; yield return null;
        }
        Check(CombatEntrySelection.Consume() == null,"No stale retry selection remains");
    }
}
