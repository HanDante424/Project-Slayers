using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitboxController : MonoBehaviour
{
    [SerializeField] private Collider2D[] hitboxes;
    [SerializeField] private AttackHitbox[] attackHitboxes;
    private readonly HashSet<HitReceiver> effectTargets = new();
    private EffectAttackSettings effectSettings;
    private float effectStartedAt;
    private bool effectMode;
    private bool windowOpened;
    private bool windowEnded;

    public bool IsDamageReady { get; private set; }
    public bool IsEffectWindowOpen { get; private set; }
    public int DetectedTargetCount => effectTargets.Count;
    // Contact notification is intentionally separate from applying valid damage data.
    public event Action<HitReceiver> EffectTargetDetected;

    private void Awake()
    {
        if (hitboxes == null || hitboxes.Length == 0)
            hitboxes = GetComponentsInChildren<Collider2D>(true);
        if (attackHitboxes == null || attackHitboxes.Length == 0)
        {
            attackHitboxes =
                GetComponentsInChildren<AttackHitbox>(true);
        }

        DisableAllHitboxes();
    }

    public void Initialize(
        float attack,
        GameObject source,
        SkillData skillData,
        int skillLevel = 1)
    {
        effectMode = false;
        foreach (AttackHitbox attackHitbox in attackHitboxes)
        {
            attackHitbox.Initialize(
                attack,
                source,
                skillData,
                skillLevel
            );
        }
    }

    public void InitializeEffect(CharacterStats stats, GameObject source, EffectAttackSettings settings)
    {
        EndEffectAttack();
        effectMode = true;
        effectSettings = settings;
        effectStartedAt = Time.time;
        windowOpened = false;
        windowEnded = false;
        if (settings == null || settings.hitPermission != EffectHitPermission.Enabled)
        {
            windowEnded = true;
            if (settings == null || settings.hitPermission == EffectHitPermission.Unspecified)
                Debug.LogWarning($"{name}: hit permission is unspecified; detection disabled.", this);
            return;
        }
        if (settings.hitboxStartTime < 0f || (settings.hitboxEndTime >= 0f && settings.hitboxEndTime <= settings.hitboxStartTime))
        {
            windowEnded = true;
            Debug.LogWarning($"{name}: invalid hitbox time window; detection disabled.", this);
            return;
        }
        IsDamageReady = source != null && stats != null && settings.skillData != null &&
            settings.skillData.TryGetHitData(settings.hitIndex, out _);
        if (!IsDamageReady)
            Debug.LogWarning($"{name}: missing attacker/CharacterStats/SkillData or invalid hitIndex ({settings.hitIndex}); contact detection only, no damage.", this);
        foreach (AttackHitbox hitbox in attackHitboxes)
            if (hitbox != null) hitbox.InitializeEffect(stats, source, settings, this);
        UpdateEffectWindow();
    }

    private void Update()
    {
        if (effectMode) UpdateEffectWindow();
    }

    private void UpdateEffectWindow()
    {
        if (windowEnded || effectSettings == null) return;
        float elapsed = Time.time - effectStartedAt;
        if (effectSettings.hitboxEndTime >= 0f && elapsed >= effectSettings.hitboxEndTime)
        {
            windowEnded = true;
            DisableAllHitboxes();
        }
        else if (!windowOpened && elapsed >= effectSettings.hitboxStartTime)
        {
            windowOpened = true;
            IsEffectWindowOpen = true;
            foreach (Collider2D hitbox in hitboxes)
                if (hitbox != null) hitbox.enabled = true;
        }
    }

    internal bool TryRegisterEffectTarget(HitReceiver receiver)
    {
        if (!IsEffectWindowOpen || !effectTargets.Add(receiver)) return false;
        EffectTargetDetected?.Invoke(receiver);
        return true;
    }

    internal bool CanProcessEffectContact()
    {
        // Physics callbacks can precede Update on the boundary frame.
        UpdateEffectWindow();
        return IsEffectWindowOpen;
    }

    public void EndEffectAttack()
    {
        DisableAllHitboxes();
        effectTargets.Clear();
        IsDamageReady = false;
        effectSettings = null;
        windowEnded = true;
        if (effectMode && attackHitboxes != null)
            foreach (AttackHitbox hitbox in attackHitboxes)
                if (hitbox != null) hitbox.ClearEffectInitialization();
    }

    private void OnDisable()
    {
        if (effectMode)
        {
            EndEffectAttack();
            EffectTargetDetected = null;
        }
    }

    public void EnableHitbox(int index)
    {
        if (effectMode) return;
        DisableAllHitboxes();

        if (index < 0 || index >= hitboxes.Length)
            return;

        hitboxes[index].enabled = true;
    }

    public void DisableAllHitboxes()
    {
        IsEffectWindowOpen = false;
        if (hitboxes == null) return;
        foreach (Collider2D hitbox in hitboxes)
        {
            if (hitbox != null)
                hitbox.enabled = false;
        }
    }
}
