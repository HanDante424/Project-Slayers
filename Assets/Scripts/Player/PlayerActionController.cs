using System.Collections;
using UnityEngine;

public class PlayerActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private Animator animator;

    [Header("Prototype Ern")]
    [SerializeField] private AnimationClip tripleSlashClip;

    private bool isActing;

    private static readonly int TripleSlashHash =
        Animator.StringToHash("TripleSlash");

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

        yield return new WaitForSeconds(
            tripleSlashClip.length
        );

        motor.SetMovementControl(
            CharacterMotor2D.MovementControl.Free
        );

        isActing = false;
    }
}