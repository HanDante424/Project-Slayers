using UnityEngine;

public class HitReceiver : MonoBehaviour
{
    [SerializeField] private GoblinAnimationController animationController;

    public void ReceiveHit()
    {
        Debug.Log($"{name} hit");

        if (animationController != null)
            animationController.PlayHit();
    }
}