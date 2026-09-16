using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class PlayerActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private Animator animator;

    [Header("Prototype Ern")]
    [SerializeField] private AnimationClip tripleSlashClip;
    [SerializeField] private SkillData tripleSlashSkillData;
    [SerializeField] private SkillCooldownUI tripleSlashCooldownUI;

    [SerializeField] private GameObject tripleSlashEffectPrefab;
    [SerializeField] private Transform effectSpawnPoint;

    private CharacterStats characterStats;
    private bool isActing;
    private float tripleSlashCooldownRemaining;

    private static readonly int TripleSlashHash =
        Animator.StringToHash("TripleSlash");

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
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

        StartTripleSlashCooldown();
        StartCoroutine(PlayTripleSlash());
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

    private IEnumerator PlayTripleSlash()
    {
        isActing = true;

        motor.SetMovementControl(
            CharacterMotor2D.MovementControl.Locked
        );

        animator.SetTrigger(TripleSlashHash);

        GameObject effect = Instantiate(
            tripleSlashEffectPrefab,
            effectSpawnPoint.position,
            effectSpawnPoint.rotation
        );

        AttackHitboxController hitboxController =
            effect.GetComponent<AttackHitboxController>();

        if (hitboxController != null)
        {
            hitboxController.Initialize(
                characterStats.FinalAttack,
                gameObject,
                tripleSlashSkillData
            );
        }

        Destroy(
            effect,
            tripleSlashClip.length
        );

        yield return new WaitForSeconds(
            tripleSlashClip.length
        );

        motor.SetMovementControl(
            CharacterMotor2D.MovementControl.Free
        );

        isActing = false;
    }
}
