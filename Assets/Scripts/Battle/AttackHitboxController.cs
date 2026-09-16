using UnityEngine;

public class AttackHitboxController : MonoBehaviour
{
    [SerializeField] private Collider2D[] hitboxes;
    [SerializeField] private AttackHitbox[] attackHitboxes;

    private void Awake()
    {
        if (attackHitboxes == null || attackHitboxes.Length == 0)
        {
            attackHitboxes =
                GetComponentsInChildren<AttackHitbox>(true);
        }

        DisableAllHitboxes();
    }

    public void Initialize(
        float attack,
        GameObject source,
        SkillData skillData)
    {
        foreach (AttackHitbox attackHitbox in attackHitboxes)
        {
            attackHitbox.Initialize(
                attack,
                source,
                skillData
            );
        }
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
