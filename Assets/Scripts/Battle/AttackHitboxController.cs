using UnityEngine;

public class AttackHitboxController : MonoBehaviour
{
    [SerializeField] private Collider2D[] hitboxes;

    private void Awake()
    {
        DisableAllHitboxes();
    }

    public void EnableHitbox(int index)
    {
        DisableAllHitboxes();

        if (index < 0 || index >= hitboxes.Length)
            return;

        hitboxes[index].enabled = true;
    }

    public void DisableAllHitboxes()
    {
        foreach (Collider2D hitbox in hitboxes)
        {
            if (hitbox != null)
                hitbox.enabled = false;
        }
    }
}