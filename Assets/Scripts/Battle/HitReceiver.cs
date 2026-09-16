using UnityEngine;

public class HitReceiver : MonoBehaviour
{
    [SerializeField] private GoblinAnimationController animationController;

    public void ReceiveHit(HitData hitData)
    {
        Debug.Log(
            $"{name} hit / damage: {hitData.damage} / hitIndex: {hitData.hitIndex}"
        );

        if (animationController != null)
            animationController.PlayHit();
    }
}