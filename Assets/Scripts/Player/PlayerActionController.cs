using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(PlayerMana))]
public class PlayerActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private Animator animator;

    [Header("Prototype Ern")]
    [Tooltip("Legacy fallback until TripleSlashSkillData has a Mechanism assigned.")]
    [SerializeField] private AnimationClip tripleSlashClip;
    [SerializeField] private SkillData tripleSlashSkillData;
    [SerializeField] private SkillCooldownUI tripleSlashCooldownUI;

    [Tooltip("Legacy fallback until TripleSlashSkillData has a Mechanism assigned.")]
    [SerializeField] private GameObject tripleSlashEffectPrefab;
    [SerializeField] private Transform effectSpawnPoint;

    private CharacterStats characterStats;
    private PlayerMana playerMana;
    private bool isActing;
    private float tripleSlashCooldownRemaining;

    private static readonly int TripleSlashHash =
        Animator.StringToHash("TripleSlash");

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        playerMana = GetComponent<PlayerMana>();
    }

    private void Update()
    {
        UpdateTripleSlashCooldown();

        if (isActing)
            return;

        if (!inputReader.ConsumeTestSkill())
            return;

        if (!motor.IsGrounded)
            return;

        if (tripleSlashCooldownRemaining > 0f)
            return;

        if (!CanExecuteTripleSlash())
            return;

        // Temporary cost: 10% of the current final maximum mana.
        float manaCost = playerMana.MaxMana * 0.1f;
        if (!playerMana.Spend(manaCost))
            return;

        StartTripleSlashCooldown();
        StartCoroutine(ExecuteSkill(tripleSlashSkillData));
    }

    private void StartTripleSlashCooldown()
    {
        float cooldown = tripleSlashSkillData.cooldown;

        if (cooldown <= 0f)
            return;

        tripleSlashCooldownRemaining = cooldown;
        tripleSlashCooldownUI?.StartCooldown(cooldown);
    }

    private void UpdateTripleSlashCooldown()
    {
        if (tripleSlashCooldownRemaining <= 0f)
            return;

        float cooldown = tripleSlashSkillData.cooldown;

        tripleSlashCooldownRemaining = Mathf.Max(
            tripleSlashCooldownRemaining - Time.deltaTime,
            0f
        );

        float elapsed = cooldown - tripleSlashCooldownRemaining;
        tripleSlashCooldownUI?.SetCooldownProgress(
            elapsed,
            cooldown
        );

        if (tripleSlashCooldownRemaining <= 0f)
            tripleSlashCooldownUI?.CompleteCooldown();
    }

    private bool CanExecuteTripleSlash()
    {
        if (tripleSlashSkillData == null)
        {
            Debug.LogWarning(
                "TripleSlash has no SkillData assigned.",
                this
            );
            return false;
        }

        if (motor == null ||
            animator == null ||
            characterStats == null ||
            effectSpawnPoint == null)
        {
            Debug.LogWarning(
                "TripleSlash execution references are incomplete.",
                this
            );
            return false;
        }

        if (tripleSlashSkillData.mechanism != null)
            return true;

        bool hasLegacyFallback =
            tripleSlashClip != null &&
            tripleSlashEffectPrefab != null;

        if (!hasLegacyFallback)
        {
            Debug.LogWarning(
                "TripleSlash has no Mechanism or complete legacy fallback.",
                this
            );
        }

        return hasLegacyFallback;
    }

    private IEnumerator ExecuteSkill(SkillData skillData)
    {
        SkillExecutionContext context = new SkillExecutionContext(
            gameObject,
            characterStats,
            animator,
            effectSpawnPoint
        );

        IEnumerator execution = skillData.mechanism != null
            ? skillData.mechanism.Execute(context, skillData)
            : ExecuteLegacyTripleSlash(context, skillData);

        isActing = true;

        motor.SetMovementControl(
            CharacterMotor2D.MovementControl.Locked
        );

        try
        {
            yield return RunSkillExecution(execution);
        }
        finally
        {
            RestoreActionState();
        }
    }

    private IEnumerator RunSkillExecution(IEnumerator execution)
    {
        while (true)
        {
            bool hasNext;
            object current = null;

            try
            {
                hasNext = execution.MoveNext();

                if (hasNext)
                    current = execution.Current;
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
                hasNext = false;
            }

            if (!hasNext)
                yield break;

            yield return current;
        }
    }

    private IEnumerator ExecuteLegacyTripleSlash(
        SkillExecutionContext context,
        SkillData skillData)
    {
        animator.SetTrigger(TripleSlashHash);

        GameObject effect = Instantiate(
            tripleSlashEffectPrefab,
            context.EffectSpawnPoint.position,
            context.EffectSpawnPoint.rotation
        );

        AttackHitboxController hitboxController =
            effect.GetComponent<AttackHitboxController>();

        if (hitboxController != null)
        {
            hitboxController.Initialize(
                context.CharacterStats.FinalAttack,
                context.Owner,
                skillData
            );
        }

        Destroy(
            effect,
            tripleSlashClip.length
        );

        yield return new WaitForSeconds(
            tripleSlashClip.length
        );
    }

    private void RestoreActionState()
    {
        if (motor != null)
        {
            motor.SetMovementControl(
                CharacterMotor2D.MovementControl.Free
            );
        }

        isActing = false;
    }

    private void OnDisable()
    {
        if (isActing)
            RestoreActionState();
    }
}
