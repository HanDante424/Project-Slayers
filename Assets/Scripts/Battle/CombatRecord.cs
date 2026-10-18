using System;
using UnityEngine;

// Per-run runtime data. Neither MapData nor character definition assets are mutated.
public sealed class CombatRecord : IDisposable
{
    private readonly GameObject player;
    private readonly double startedAt;
    private double stoppedAt;
    public bool IsRecording { get; private set; } = true;
    public long DamageDealt { get; private set; }
    public long DamageReceived { get; private set; }
    public int Kills { get; private set; }
    public double ElapsedSeconds => (IsRecording ? Time.timeAsDouble : stoppedAt) - startedAt;

    public CombatRecord(GameObject player)
    {
        this.player = player;
        startedAt = Time.timeAsDouble;
        Health.DamageApplied += OnDamage;
    }

    private void OnDamage(Health target, GameObject source, int actualDamage, bool killed)
    {
        if (!IsRecording || target == null || player == null || target.gameObject.scene != player.scene) return;
        bool isPlayer = target.transform == player.transform || target.transform.IsChildOf(player.transform);
        bool isEnemy = !isPlayer && target.GetComponent<HitReceiver>() != null;
        bool fromPlayer = source != null &&
            (source.transform == player.transform || source.transform.IsChildOf(player.transform));
        if (isPlayer) DamageReceived += actualDamage;
        if (isEnemy && fromPlayer) DamageDealt += actualDamage;
        // Health rejects subsequent damage while dead. ResetHealth starts a new pooled life.
        if (isEnemy && killed) Kills++;
    }

    public void Stop()
    {
        if (!IsRecording) return;
        stoppedAt = Time.timeAsDouble;
        IsRecording = false;
        Health.DamageApplied -= OnDamage;
    }

    public void Dispose() => Stop();

    public static string FormatTime(double seconds)
    {
        long total = (long)Math.Max(0, Math.Floor(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }
}

public sealed class CombatResultData
{
    public MapData Stage { get; }
    public bool IsTestClear { get; }
    public double ElapsedSeconds { get; }
    public int Kills { get; }
    public long DamageDealt { get; }
    public long DamageReceived { get; }
    // No reward receipt is fabricated while actual granting/inventory are unimplemented.
    public CombatResultData(MapData stage, bool isTestClear, CombatRecord record)
    {
        Stage = stage; IsTestClear = isTestClear;
        ElapsedSeconds = record.ElapsedSeconds; Kills = record.Kills;
        DamageDealt = record.DamageDealt; DamageReceived = record.DamageReceived;
    }
}
