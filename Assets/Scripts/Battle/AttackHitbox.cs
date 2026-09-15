using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GoblinEnemy goblin =
            other.GetComponentInParent<GoblinEnemy>();

        if (goblin == null)
            return;

        goblin.ReceiveTestHit();
    }
}