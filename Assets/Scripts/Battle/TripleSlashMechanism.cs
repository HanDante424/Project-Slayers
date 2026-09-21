using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TripleSlashMechanism",
    menuName = "Slayers/Skill Mechanism/Triple Slash")]
public class TripleSlashMechanism : SkillMechanism
{
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private string animatorTrigger = "TripleSlash";

    public override IEnumerator Execute(
        SkillExecutionContext context,
        SkillData skillData)
    {
        if (!CanExecute(context, skillData))
            yield break;

        int triggerHash = Animator.StringToHash(animatorTrigger);
        context.Animator.SetTrigger(triggerHash);

        GameObject effect = Instantiate(
            effectPrefab,
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

        Destroy(effect, animationClip.length);

        yield return new WaitForSeconds(animationClip.length);
    }

    public override bool CanExecute(
        SkillExecutionContext context,
        SkillData skillData)
    {
        if (context == null)
        {
            Debug.LogError("TripleSlash requires a SkillExecutionContext.", this);
            return false;
        }

        if (skillData == null)
        {
            Debug.LogError("TripleSlash requires SkillData.", this);
            return false;
        }

        if (context.Owner == null ||
            context.CharacterStats == null ||
            context.Animator == null ||
            context.EffectSpawnPoint == null)
        {
            Debug.LogError(
                "TripleSlash execution context has missing references.",
                this
            );
            return false;
        }

        if (animationClip == null || effectPrefab == null)
        {
            Debug.LogError(
                "TripleSlash mechanism has missing asset references.",
                this
            );
            return false;
        }

        if (string.IsNullOrWhiteSpace(animatorTrigger))
        {
            Debug.LogError(
                "TripleSlash mechanism requires an Animator trigger.",
                this
            );
            return false;
        }

        return true;
    }
}
