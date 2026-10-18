using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CombatResultController : MonoBehaviour
{
    [SerializeField] private CombatPlayerEntry playerEntry;
    [SerializeField] private CombatResultView view;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private Button spawnButton;
    [SerializeField] private Button testClearButton;
    private readonly Dictionary<Behaviour, bool> pausedBehaviours = new();
    private readonly Dictionary<Health, bool> pausedHealth = new();
    private float previousTimeScale;
    private bool ownsPause, loading;
    private Button retryButton, townButton;
    public CombatRecord Record { get; private set; }
    public CombatResultData Result { get; private set; }

    private void Awake() => view.ResetView();

    private void Start()
    {
        Record = new CombatRecord(playerEntry.ActivePlayer);
        retryButton = view.RetryButton;
        townButton = view.TownButton;
        retryButton.onClick.AddListener(Retry);
        townButton.onClick.AddListener(ReturnTown);
    }

    // Stage-specific objective systems can call this once they define a real completion condition.
    // Kills do not automatically complete Search/Defense/Attack/Conquest/Raid.
    public void CompleteStage() => Complete(false);
    public void CompleteForTesting() => Complete(true);

    private void Complete(bool test)
    {
        if (loading || Result != null || Record == null) return;
        Record.Stop();
        Result = new CombatResultData(playerEntry.SelectedMap, test, Record);
        FreezeCombat();
        view.Show(Result);
    }

    private void FreezeCombat()
    {
        previousTimeScale = Time.timeScale;
        ownsPause = true;
        Time.timeScale = 0f;
        var player = playerEntry.ActivePlayer;
        if (player != null)
            foreach (var behaviour in player.GetComponents<MonoBehaviour>())
                if (behaviour is PlayerInput || behaviour is PlayerInputReader ||
                    behaviour is PlayerActionController || behaviour is CharacterMotor2D ||
                    behaviour is ICharacterMechanism)
                { pausedBehaviours[behaviour] = behaviour.enabled; behaviour.enabled = false; }
        // UI remains live at timeScale=0. Explicit damage gates also reject direct hit calls.
        foreach (var health in FindObjectsByType<Health>(FindObjectsSortMode.None))
            if (health.gameObject.scene == gameObject.scene)
            { pausedHealth[health] = health.DamageEnabled; health.DamageEnabled = false; }
        foreach (var pool in FindObjectsByType<GoblinPool>(FindObjectsSortMode.None))
            if (pool.gameObject.scene == gameObject.scene)
            { pausedBehaviours[pool] = pool.enabled; pool.enabled = false; }
        pausedBehaviours[spawnButton] = spawnButton.enabled;
        pausedBehaviours[testClearButton] = testClearButton.enabled;
        spawnButton.enabled = false;
        testClearButton.enabled = false;
    }

    private void ReleasePause()
    {
        if (!ownsPause) return;
        ownsPause = false;
        Time.timeScale = previousTimeScale;
        foreach (var entry in pausedHealth) if (entry.Key != null) entry.Key.DamageEnabled = entry.Value;
        foreach (var entry in pausedBehaviours) if (entry.Key != null) entry.Key.enabled = entry.Value;
        pausedHealth.Clear(); pausedBehaviours.Clear();
    }

    public void Retry()
    {
        if (loading || Result == null || !Application.CanStreamedLevelBeLoaded("Dev_Combat")) return;
        if (!playerEntry.PrepareRetry())
        { Debug.LogError("[CombatResult] Retry selection is invalid.", this); return; }
        loading = true;
        view.SetButtonsEnabled(false);
        Record?.Stop();
        ReleasePause();
        sceneLoader.ReloadCurrentScene();
    }

    public void ReturnTown()
    {
        if (loading || !Application.CanStreamedLevelBeLoaded("Town")) return;
        loading = true;
        view.SetButtonsEnabled(false);
        Record?.Stop();
        CombatEntrySelection.Clear();
        ReleasePause();
        sceneLoader.LoadTown();
    }

    private void OnDisable()
    {
        Record?.Dispose();
        ReleasePause();
    }

    private void OnDestroy()
    {
        if (retryButton != null) retryButton.onClick.RemoveListener(Retry);
        if (townButton != null) townButton.onClick.RemoveListener(ReturnTown);
    }
}
