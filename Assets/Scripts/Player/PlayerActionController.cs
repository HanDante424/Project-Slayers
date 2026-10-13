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
    [Tooltip("Optional character-specific input handler. Leave empty for Prototype Ern.")]
    [SerializeField] private MonoBehaviour characterMechanismBehaviour;

    private ICharacterMechanism characterMechanism;

    [Header("Prototype Ern")]
    [Tooltip("Legacy fallback until TripleSlashSkillData has a Mechanism assigned.")]
    [SerializeField] private AnimationClip tripleSlashClip;
    [SerializeField] private SkillData tripleSlashSkillData;
    [SerializeField] private LearnedSkill tripleSlashSkill = new();
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
        characterMechanism = characterMechanismBehaviour as ICharacterMechanism;
        if (characterMechanismBehaviour != null && characterMechanism == null)
            Debug.LogError("Character mechanism must implement ICharacterMechanism.", this);

        characterStats = GetComponent<CharacterStats>();
        playerMana = GetComponent<PlayerMana>();
        if (tripleSlashSkill == null)
            tripleSlashSkill = new LearnedSkill();

        tripleSlashSkill.Bind(tripleSlashSkillData);
    }

    private void Update()
    {
        UpdateTripleSlashCooldown();

        bool basicAttackPressed = inputReader.ConsumeBasicAttack();
        if (characterMechanism != null)
        {
            // The assigned character owns its actions; the Ern test skill stays a fallback.
            inputReader.ConsumeTestSkill();
            if (!isActing && characterMechanismBehaviour.isActiveAndEnabled && basicAttackPressed)
                characterMechanism.RequestBasicAttack();
            return;
        }

        if (isActing)
            return;

        if (!inputReader.ConsumeTestSkill())
            return;

        if (!motor.IsGrounded)
            return;

        if (tripleSlashCooldownRemaining > 0f)
            return;

        if (tripleSlashSkillData == null)
        {
            Debug.LogWarning(
                "TripleSlash has no SkillData assigned.",
                this
            );
            return;
        }

        SkillExecutionContext context = CreateSkillExecutionContext();

        if (!CanExecuteTripleSlash(context))
            return;

        // Temporary cost: 10% of the current final maximum mana.
        float manaCost = playerMana.MaxMana * 0.1f;
        if (!playerMana.Spend(manaCost))
            return;

        StartTripleSlashCooldown();
        StartCoroutine(ExecuteSkill(tripleSlashSkillData, context));
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

    private SkillExecutionContext CreateSkillExecutionContext()
    {
        return new SkillExecutionContext(
            gameObject,
            characterStats,
            animator,
            effectSpawnPoint,
            tripleSlashSkill.CurrentLevel
        );
    }

    private bool CanExecuteTripleSlash(
        SkillExecutionContext context)
    {
        if (tripleSlashSkillData == null)
        {
            Debug.LogWarning(
                "TripleSlash has no SkillData assigned.",
                this
            );
            return false;
        }

        if (motor == null)
        {
            Debug.LogWarning(
                "TripleSlash requires a CharacterMotor2D reference.",
                this
            );
            return false;
        }

        if (tripleSlashSkillData.mechanism != null)
        {
            return tripleSlashSkillData.mechanism.CanExecute(
                context,
                tripleSlashSkillData
            );
        }

        bool hasExecutionContext =
            context != null &&
            context.Owner != null &&
            context.CharacterStats != null &&
            context.Animator != null &&
            context.EffectSpawnPoint != null;

        bool hasLegacyFallback =
            hasExecutionContext &&
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

    private IEnumerator ExecuteSkill(
        SkillData skillData,
        SkillExecutionContext context)
    {
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
                skillData,
                context.SkillLevel
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
        characterMechanism?.CancelAction();
        if (isActing)
            RestoreActionState();
    }
}
