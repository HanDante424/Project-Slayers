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

    [SerializeField] private GameObject tripleSlashEffectPrefab;
    [SerializeField] private Transform effectSpawnPoint;

    private CharacterStats characterStats;
    private bool isActing;

    private static readonly int TripleSlashHash =
        Animator.StringToHash("TripleSlash");

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
    }

    private void Update()
    {
        if (isActing)
            return;

        if (!inputReader.ConsumeTestSkill())
            return;

        if (!motor.IsGrounded)
            return;

        StartCoroutine(PlayTripleSlash());
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
                gameObject
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
