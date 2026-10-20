using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VerifyCameraBounds
{
    [MenuItem("Tools/Slayers/Verify Camera Bounds %#F6")]
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
        new GameObject("Temporary camera bounds verification").AddComponent<CameraBoundsVerification>();
    }
}

public sealed class CameraBoundsVerification : MonoBehaviour
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private int checks, errors;
    private object Read(CombatPlayerEntry entry, string field) => typeof(CombatPlayerEntry).GetField(field, Fields).GetValue(entry);
    private void Write(CombatPlayerEntry entry, string field, object value) => typeof(CombatPlayerEntry).GetField(field, Fields).SetValue(entry, value);
    private void Step(CombatPlayerEntry entry, float x, int facing, float seconds)
        => typeof(CombatPlayerEntry).GetMethod("UpdateHorizontalFraming", Fields).Invoke(entry, new object[] { x, facing, seconds });
    private void ResetFraming(CombatPlayerEntry entry)
    {
        Write(entry, "observedFacingDirection", 1); Write(entry, "framingDirection", 1);
        foreach (var name in new[] { "lastPlayerX", "facingChangeX" }) Write(entry, name, 0f);
        foreach (var name in new[] { "framingX", "framingFromX", "framingTargetX" }) Write(entry, name, 0.3f);
        Write(entry, "framingElapsed", 0.2f);
    }
    private void VerifyDirectionalFraming(CombatPlayerEntry entry)
    {
        Near((float)Read(entry, "framingTurnDistance"), 3, "Authored turn threshold is 3 world units");
        Near((float)Read(entry, "framingTransitionDuration"), 0.2f, "Authored transition duration is 0.2s");
        ResetFraming(entry);
        Step(entry, 0, -1, 0.1f); Near((float)Read(entry, "framingX"), 0.3f, "Stationary turn retains previous framing");
        Step(entry, -2.99f, -1, 0.1f); Near((float)Read(entry, "framingX"), 0.3f, "Below threshold does not shift framing");
        Step(entry, -3, -1, 0); Near((float)Read(entry, "framingX"), 0.3f, "Threshold starts transition without snapping");
        Step(entry, -3, -1, 0.056f);
        float early = (float)Read(entry, "framingX");
        Check(early > 0.62f && early < 0.7f, "First 28% of time covers roughly 80% of shift, without overshoot");
        Step(entry, -3, -1, 0.056f);
        float middle = (float)Read(entry, "framingX");
        Check(middle - early < (early - 0.3f) * 0.25f, "After 80% shift, equal time advances much less distance");
        Step(entry, -3, -1, 0.088f); Near((float)Read(entry, "framingX"), 0.7f, "Left-facing framing completes exactly at 0.2s");
        Step(entry, -3, 1, 0); Step(entry, -0.01f, 1, 0.1f);
        Near((float)Read(entry, "framingX"), 0.7f, "Right turn also waits for 3 units");
        Step(entry, 0, 1, 0.2f); Near((float)Read(entry, "framingX"), 0.3f, "Right-facing framing mirrors to 30%");
        ResetFraming(entry);
        Step(entry, 0, -1, 0); Step(entry, -2, -1, 0); Step(entry, -2, 1, 0); Step(entry, 0, 1, 0);
        Step(entry, 0, -1, 0); Step(entry, -2, -1, 0.2f);
        Near((float)Read(entry, "framingX"), 0.3f, "Repeated short reversals do not accumulate distance");
        Write(entry, "framingTurnDistance", 1f); Write(entry, "framingTransitionDuration", 0.4f);
        ResetFraming(entry); Step(entry, 0, -1, 0); Step(entry, -1, -1, 0.2f);
        Check((float)Read(entry, "framingX") < 0.7f, "Inspector duration changes timing");
        Step(entry, -1, -1, 0.2f); Near((float)Read(entry, "framingX"), 0.7f, "Inspector threshold and duration applied");
        Write(entry, "framingTransitionDuration", 0f); Step(entry, -1, 1, 0); Step(entry, 0, 1, 0);
        Near((float)Read(entry, "framingX"), 0.3f, "Zero duration is safe");
        Write(entry, "framingTurnDistance", 3f); Write(entry, "framingTransitionDuration", 0.2f);
        typeof(CombatPlayerEntry).GetMethod("ConfigureCamera", Fields).Invoke(entry, null);
    }
    private void Check(bool value, string label) { checks++; if (!value) throw new InvalidOperationException(label); }
    private void Near(float value, float expected, string label) => Check(Mathf.Abs(value - expected) < 0.002f, label + ": " + value + " expected " + expected);
    private void Log(string text, string trace, LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors++; }
    private void Inside(Camera camera, Bounds bounds)
    {
        Vector3 p = camera.transform.position;
        float h = camera.orthographicSize, w = h * camera.aspect;
        Check(p.x - w >= bounds.min.x - 0.002f && p.x + w <= bounds.max.x + 0.002f, "Entire horizontal viewport inside rectangle");
        Check(p.y - h >= bounds.min.y - 0.002f && p.y + h <= bounds.max.y + 0.002f, "Entire vertical viewport inside rectangle");
    }
    private IEnumerator Start()
    {
        Application.logMessageReceived += Log;
        var body = Body(); bool failed = false;
        while (true)
        {
            object next;
            try { if (!body.MoveNext()) break; next = body.Current; }
            catch (Exception exception) { failed = true; Debug.LogError("[CameraBoundsTest] FAIL " + exception); break; }
            yield return next;
        }
        Application.logMessageReceived -= Log;
        Debug.Log($"[CameraBoundsTest] RESULT checks={checks}, errors={errors}, failed={failed}");
        EditorApplication.isPlaying = false;
    }
    private IEnumerator Body()
    {
        yield return new WaitForEndOfFrame();
        var limiter = FindFirstObjectByType<OrthographicCameraBounds>();
        var entry = FindFirstObjectByType<CombatPlayerEntry>();
        Check(limiter != null && entry != null && entry.ActivePlayer != null, "Authored camera limiter and standalone player connected");
        VerifyDirectionalFraming(entry);
        var camera = limiter.GetComponent<Camera>();
        var box = (BoxCollider2D)typeof(OrthographicCameraBounds).GetField("cameraBounds", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(limiter);
        Check(box != null && box.name == "CameraBounds" && !box.enabled && box.isTrigger, "Existing CameraBounds contains definition-only collider, no physics contacts");
        var bounds = limiter.GetWorldBounds();
        var marker = box.GetComponent<SpriteRenderer>();
        Near(bounds.size.x, marker.sprite.bounds.size.x * Mathf.Abs(box.transform.lossyScale.x), "Collider matches authored marker width");
        Near(bounds.size.y, marker.sprite.bounds.size.y * Mathf.Abs(box.transform.lossyScale.y), "Collider matches authored marker height");
        Inside(camera, bounds);
        Vector3 playerPosition = entry.ActivePlayer.transform.position;
        var rb = entry.ActivePlayer.GetComponent<Rigidbody2D>(); rb.simulated = false;
        Vector3 initialBoxPosition = box.transform.position;
        Vector2 initialSize = box.size, initialOffset = box.offset;
        float z = camera.transform.position.z;
        var viewport = (Vector2)typeof(CombatPlayerEntry).GetField("playerViewportPosition", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(entry);
        Vector3 centeredPlayer = bounds.center + new Vector3((viewport.x - 0.5f) * 2 * camera.orthographicSize * camera.aspect, (viewport.y - 0.5f) * 2 * camera.orthographicSize, 0);
        centeredPlayer.z = playerPosition.z;
        entry.ActivePlayer.transform.position = centeredPlayer;
        yield return null; yield return new WaitForEndOfFrame();
        Near(camera.WorldToViewportPoint(centeredPlayer).x, viewport.x, "Existing central horizontal player framing preserved");
        Near(camera.WorldToViewportPoint(centeredPlayer).y, viewport.y, "Existing central vertical player framing preserved");
        var motor = entry.ActivePlayer.GetComponent<CharacterMotor2D>();
        bool motorEnabled = motor.enabled; motor.enabled = false;
        Vector3 playerScale = entry.ActivePlayer.transform.localScale;
        box.size = initialSize * 3f;
        entry.ActivePlayer.transform.localScale = new Vector3(-Mathf.Abs(playerScale.x), playerScale.y, playerScale.z);
        yield return null; yield return new WaitForEndOfFrame();
        Near(camera.WorldToViewportPoint(entry.ActivePlayer.transform.position).x, 0.3f, "Actual stationary left turn keeps right-facing framing");
        entry.ActivePlayer.transform.position += Vector3.left * 2.9f;
        yield return null; yield return new WaitForEndOfFrame();
        Near(camera.WorldToViewportPoint(entry.ActivePlayer.transform.position).x, 0.3f, "Actual travel below threshold keeps old framing");
        entry.ActivePlayer.transform.position += Vector3.left * 0.2f;
        yield return new WaitForSeconds(0.25f); yield return new WaitForEndOfFrame();
        Near(camera.WorldToViewportPoint(entry.ActivePlayer.transform.position).x, 0.7f, "Actual LateUpdate reaches left 70% framing after threshold");
        Inside(camera, limiter.GetWorldBounds());
        entry.ActivePlayer.transform.localScale = new Vector3(Mathf.Abs(playerScale.x), playerScale.y, playerScale.z);
        yield return null; yield return new WaitForEndOfFrame();
        entry.ActivePlayer.transform.position += Vector3.right * 3.1f;
        yield return new WaitForSeconds(0.25f); yield return new WaitForEndOfFrame();
        Near(camera.WorldToViewportPoint(entry.ActivePlayer.transform.position).x, 0.3f, "Actual LateUpdate reaches right 30% framing after threshold");
        Near(camera.WorldToViewportPoint(entry.ActivePlayer.transform.position).y, viewport.y, "Directional transition leaves vertical follow unchanged");
        box.size = initialSize;
        foreach (var point in new[] { new Vector2(-100, 0), new Vector2(100, 0), new Vector2(0, -100), new Vector2(0, 100), new Vector2(100, 100) })
        {
            entry.ActivePlayer.transform.position = new Vector3(point.x, point.y, playerPosition.z);
            yield return null; yield return new WaitForEndOfFrame();
            Inside(camera, limiter.GetWorldBounds()); Near(camera.transform.position.z, z, "Camera Z retained");
            Vector3 previous = camera.transform.position;
            yield return null; yield return new WaitForEndOfFrame();
            Near(camera.transform.position.x, previous.x, "No horizontal follow/clamp oscillation");
            Near(camera.transform.position.y, previous.y, "No vertical follow/clamp oscillation");
            Near(entry.ActivePlayer.transform.position.x, point.x, "Player remains free outside camera area");
        }
        box.size = initialSize * 1.5f; box.offset = new Vector2(0.2f, -0.1f);
        box.transform.position += new Vector3(3, 2, 0);
        yield return null; yield return new WaitForEndOfFrame();
        Inside(camera, limiter.GetWorldBounds());
        Near(limiter.GetWorldBounds().center.x, box.transform.TransformPoint(box.offset).x, "Live bounds position/offset update");
        camera.orthographicSize = 3; camera.aspect = 4f / 3f;
        yield return null; yield return new WaitForEndOfFrame(); Inside(camera, limiter.GetWorldBounds());
        camera.aspect = 21f / 9f;
        yield return null; yield return new WaitForEndOfFrame(); Inside(camera, limiter.GetWorldBounds());
        box.size = new Vector2(0.01f, 0.01f);
        yield return null; yield return new WaitForEndOfFrame();
        Near(camera.transform.position.x, limiter.GetWorldBounds().center.x, "Undersized horizontal area centers camera safely");
        Near(camera.transform.position.y, limiter.GetWorldBounds().center.y, "Undersized vertical area centers camera safely");
        box.size = initialSize; box.offset = initialOffset; box.transform.position = initialBoxPosition;
        camera.orthographicSize = 5; camera.ResetAspect();
        camera.transform.position = new Vector3(-1000, 1000, z); limiter.ConstrainPosition();
        Inside(camera, limiter.GetWorldBounds());
        entry.ActivePlayer.transform.position = playerPosition; rb.simulated = true;
        entry.ActivePlayer.transform.localScale = playerScale; motor.enabled = motorEnabled;
    }
}
