using UnityEngine;

// One-shot presentation lifetime, independent of the action that spawned it.
public class DestroyAfterAnimatorPlayback : MonoBehaviour
{
    private Animator effectAnimator;
    private bool completed;

    private void Awake()
    {
        effectAnimator = GetComponent<Animator>();
        if (effectAnimator == null || effectAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("One-shot effect requires an Animator Controller.", this);
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (effectAnimator == null || !effectAnimator.isActiveAndEnabled)
            return;

        // Give the final sampled pose a rendered frame before removal.
        if (completed)
        {
            Destroy(gameObject);
            return;
        }
        completed = effectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
}
