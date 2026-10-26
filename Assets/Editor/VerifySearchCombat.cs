using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class VerifySearchCombat
{
    [MenuItem("Tools/Slayers/Verify Search Combat %#F5")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Search_Combat.unity");
        EditorApplication.playModeStateChanged += Entered;
        EditorApplication.isPlaying = true;
    }
    private static void Entered(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= Entered;
        var root = new GameObject("Temporary Search scene verification");
        UnityEngine.Object.DontDestroyOnLoad(root); root.AddComponent<SearchCombatVerification>();
    }
}

public sealed class SearchCombatVerification : MonoBehaviour
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private int checks, errors;
    private void Check(bool valid, string label) { checks++; if (!valid) throw new InvalidOperationException(label); }
    private object Read(object target, string field) => target.GetType().GetField(field, Fields).GetValue(target);
    private void Log(string message, string trace, LogType type) { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) errors++; }
    private void Inside(Camera camera, Bounds bounds)
    {
        var position = camera.transform.position;
        float h = camera.orthographicSize, w = h * camera.aspect;
        Check(position.x - w >= bounds.min.x - 0.002f && position.x + w <= bounds.max.x + 0.002f &&
            position.y - h >= bounds.min.y - 0.002f && position.y + h <= bounds.max.y + 0.002f, "Entire camera viewport confined");
    }
    private IEnumerator Start()
    {
        Application.logMessageReceived += Log;
        var body = Body(); bool failed = false;
        while (true)
        {
            object next;
            try { if (!body.MoveNext()) break; next = body.Current; }
            catch (Exception exception) { failed = true; Debug.LogError("[SearchCombatTest] FAIL " + exception); break; }
            yield return next;
        }
        Application.logMessageReceived -= Log;
        Debug.Log($"[SearchCombatTest] RESULT checks={checks}, errors={errors}, failed={failed}");
        EditorApplication.isPlaying = false;
    }
    private IEnumerator Body()
    {
        int searchTileCount = -1;
        foreach (string sceneName in new[] { "Search_Combat", "Dev_Combat" })
        {
            if (SceneManager.GetActiveScene().name != sceneName) SceneManager.LoadScene(sceneName);
            yield return null; yield return null;
            Check(SceneManager.GetActiveScene().name == sceneName, sceneName + " loads");
            var entry = FindFirstObjectByType<CombatPlayerEntry>();
            Check(entry != null && entry.ActivePlayer != null && entry.ActivePlayer.activeInHierarchy, sceneName + " standalone player prepared");
            var player = entry.ActivePlayer;
            var motor = player.GetComponent<CharacterMotor2D>();
            var reader = player.GetComponent<PlayerInputReader>();
            var input = player.GetComponent<PlayerInput>();
            input.enabled = false;
            var move = typeof(PlayerInputReader).GetField("<MoveInput>k__BackingField", Fields);
            Check(move != null && motor.enabled && player.GetComponent<PlayerActionController>().enabled, "Existing movement/input/action components preserved");
            for (int i = 0; i < 70; i++) { move.SetValue(reader, Vector2.zero); yield return new WaitForFixedUpdate(); }
            Check(motor.IsGrounded, sceneName + " actual floor collision and ground detection");
            float start = player.transform.position.x;
            var limiter = FindFirstObjectByType<OrthographicCameraBounds>();
            var camera = (Camera)Read(entry, "combatCamera");
            Check(limiter != null && camera != null && camera.orthographic, "Existing camera and limiter references");
            Inside(camera, limiter.GetWorldBounds());
            float cameraStartX = camera.transform.position.x;
            for (int i = 0; i < 20; i++)
            {
                move.SetValue(reader, Vector2.right); yield return new WaitForFixedUpdate();
                Inside(camera, limiter.GetWorldBounds());
            }
            move.SetValue(reader, Vector2.zero);
            Check(player.transform.position.x > start + 0.2f, sceneName + " actual motor movement");
            Check(camera.transform.position.x > cameraStartX + 0.2f, sceneName + " camera follows actual moving player");
            var background = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Single(r => r.name == "BackDev");
            Check(background.enabled && background.sprite != null, "Background sprite is loaded and enabled");
            var context = FindFirstObjectByType<CombatSceneDefinition>();
            if (sceneName == "Search_Combat")
            {
                var stage = AssetDatabase.LoadAssetAtPath<MapData>("Assets/SOData/StageData/Search/Stage_001_GoblinHamlet.asset");
                Check(context != null && context.Stage == stage && stage.stageCode == "001", "Scene has static GoblinHamlet MapData reference");
                Check(entry.SelectedMap == null, "Static context does not execute or replace per-run selection");
                Check(background.sprite == AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PrototypeAsset/Map/Stage_001_GoblinHamlet/Stage_001_GoblinHamlet_BackGround.png"), "Correct full background sprite reference");
                Check(Vector3.Distance(background.bounds.center, limiter.GetWorldBounds().center) < 0.002f &&
                    Vector2.Distance(background.bounds.size, limiter.GetWorldBounds().size) < 0.002f, "CameraBounds matches actual world background rectangle");
                Check(stage.goldReward == 100 && stage.memoryFragmentReward == 100 && stage.enemies.Count == 1, "Existing authored stage values unchanged");
            }
            else Check(context == null, "Original Dev scene has no new stage-context component");
            var tilemap = FindFirstObjectByType<Tilemap>();
            Check(tilemap != null && tilemap.GetUsedTilesCount() > 0, "Existing nonempty Tilemap retained");
            if (sceneName == "Search_Combat") searchTileCount = tilemap.GetUsedTilesCount();
            else Check(tilemap.GetUsedTilesCount() == searchTileCount, "Search and Dev retain same tile references/count");
            var hud = (Image)Read(entry, "portraitImage");
            Check(hud != null && hud.sprite != null && hud.isActiveAndEnabled && Read(entry, "healthUI") != null && Read(entry, "manaUI") != null, "HUD references and active portrait retained");
            var results = FindFirstObjectByType<CombatResultController>();
            var spawn = (Button)Read(results, "spawnButton");
            Check(spawn != null && spawn.IsActive() && spawn.onClick.GetPersistentMethodName(0) == nameof(GoblinPool.SpawnGoblin), "Existing spawn test button binding retained");
            ExecuteEvents.Execute(spawn.gameObject, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            yield return null;
            var pool = FindFirstObjectByType<GoblinPool>();
            var goblin = (GoblinEnemy)Read(pool, "activeGoblin");
            Check(goblin != null && goblin.gameObject.activeInHierarchy, "Existing pool spawns goblin");
            pool.SpawnGoblin(); Check(ReferenceEquals(Read(pool, "activeGoblin"), goblin), "Existing pool rejects duplicate spawn");
            var rb = player.GetComponent<Rigidbody2D>(); rb.simulated = false;
            motor.enabled = false;
            foreach (var point in new[] { new Vector2(-100, 0), new Vector2(100, 0), new Vector2(0, -100), new Vector2(0, 100) })
            {
                player.transform.position = new Vector3(point.x, point.y, player.transform.position.z);
                yield return null; yield return new WaitForEndOfFrame();
                Inside(camera, limiter.GetWorldBounds());
            }
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) == 0, "No Missing Script: " + transform.name);
            Debug.Log("[SearchCombatTest] Verified " + sceneName + " movement, ground, background, bounds, HUD and pool.");
        }
    }
}
