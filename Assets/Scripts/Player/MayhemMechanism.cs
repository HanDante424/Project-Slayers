using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MayhemMechanism : MonoBehaviour, ICharacterMechanism
{
    [Serializable]
    private sealed class BloodSwordStep
    {
        public string animatorTrigger;
        public string animatorState;
        public AnimationClip animationClip;
        public GameObject effectPrefab;
        public AnimationClip effectClip;
        public CharacterMotor2D.MovementControl movementControl =
            CharacterMotor2D.MovementControl.Free;

        public BloodSwordStep(string suffix)
        {
            animatorTrigger = "BloodSword" + suffix;
            animatorState = "Playerable_Mayhem_Skill_Animation_004_BloodSword_" + suffix;
        }
    }

    [Header("References")]
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform effectSpawnPoint;

    [Header("Blood Sword")]
    [SerializeField] private BloodSwordStep bloodSword00 = new("00");
    [SerializeField] private BloodSwordStep bloodSword01 = new("01");
    [SerializeField] private BloodSwordStep bloodSword02 = new("02");

    private const float ComboWindow = 1f;
    private readonly List<GameObject> effects = new();
    private BloodSwordStep nextStep;
    private BloodSwordStep activeStep;
    private Coroutine attackCoroutine;
    private float lastAttackStartedAt = float.NegativeInfinity;
    private bool attackQueued;
    private float queuedInputAt;
    private bool ownsMovementControl;
    private CharacterMotor2D.MovementControl previousMovementControl;

    public bool IsActing => activeStep != null;

    public void RequestBasicAttack()
    {
        if (!isActiveAndEnabled || motor == null)
            return;

        if (IsActing)
        {
            if (!attackQueued)
            {
                attackQueued = true;
                queuedInputAt = Time.time;
            }
            return;
        }

        if (BeginAttack(Time.time))
            attackCoroutine = StartCoroutine(RunAttack());
    }

    private bool BeginAttack(float inputAt)
    {
        // Recheck at execution time, including buffered input after the previous motion.
        if (!motor.IsGrounded)
            return false;

        BloodSwordStep step = nextStep == null || inputAt - lastAttackStartedAt > ComboWindow
            ? bloodSword00
            : nextStep;

        if (!HasRequiredReferences(step))
        {
            Debug.LogWarning("Blood Sword requires its motion, effect, Animator and spawn references.", this);
            return false;
        }

        previousMovementControl = motor.CurrentMovementControl;
        ownsMovementControl = true;
        activeStep = step;
        motor.SetMovementControl(step.movementControl);

        try
        {
            animator.SetTrigger(step.animatorTrigger);
            SpawnEffect(step);
        }
        catch (Exception exception)
        {
            CleanupAttack();
            Debug.LogException(exception, this);
            return false;
        }

        lastAttackStartedAt = Time.time;
        nextStep = step == bloodSword00 ? bloodSword01
            : step == bloodSword01 ? bloodSword02
            : bloodSword00;
        return true;
    }

    private bool HasRequiredReferences(BloodSwordStep step)
    {
        return step != null && animator != null && animator.isActiveAndEnabled &&
            animator.runtimeAnimatorController != null && effectSpawnPoint != null &&
            step.animationClip != null && step.animationClip.length > 0f &&
            step.effectPrefab != null && step.effectClip != null && step.effectClip.length > 0f &&
            !string.IsNullOrEmpty(step.animatorTrigger) &&
            !string.IsNullOrEmpty(step.animatorState) &&
            animator.HasState(0, Animator.StringToHash("Base Layer." + step.animatorState));
    }

    private IEnumerator RunAttack()
    {
        while (IsActing)
        {
            // Gameplay owns completion. Waiting starts at frame end so a buffered trigger
            // cannot cut off the last rendered frame of the current motion.
            yield return new WaitForSeconds(activeStep.animationClip.length);

            bool runQueuedAttack = attackQueued;
            float inputAt = queuedInputAt;
            attackQueued = false;
            CompleteAction();

            if (!runQueuedAttack || !BeginAttack(inputAt))
                break;
        }

        attackCoroutine = null;
    }

    private void SpawnEffect(BloodSwordStep step)
    {
        effects.RemoveAll(effect => effect == null);
        GameObject effect = Instantiate(
            step.effectPrefab, effectSpawnPoint.position, effectSpawnPoint.rotation);
        effects.Add(effect);

        Vector3 scale = effect.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (motor.transform.localScale.x < 0f ? -1f : 1f);
        effect.transform.localScale = scale;
        Destroy(effect, step.effectClip.length);
    }

    private void CompleteAction()
    {
        CleanupAttack();
    }

    public void CancelAction()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackQueued = false;
        CleanupAttack();
        foreach (GameObject effect in effects)
        {
            if (effect != null)
                Destroy(effect);
        }
        effects.Clear();
    }

    private void CleanupAttack()
    {
        if (activeStep != null && animator != null)
            animator.ResetTrigger(activeStep.animatorTrigger);

        if (ownsMovementControl && motor != null)
            motor.SetMovementControl(previousMovementControl);

        ownsMovementControl = false;
        activeStep = null;
    }

    private void OnDisable()
    {
        CancelAction();
    }
}
