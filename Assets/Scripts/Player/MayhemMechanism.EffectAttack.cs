using UnityEngine;

public partial class MayhemMechanism
{
    private CharacterStats effectAttackStats;

    private void InitializeMayhemEffectAttack(GameObject effect, EffectAttackSettings settings)
    {
        AttackHitboxController controller = effect.GetComponent<AttackHitboxController>();
        if (controller == null)
        {
            if (settings == null || settings.hitPermission != EffectHitPermission.Disabled)
                Debug.LogWarning($"{effect.name}: no existing Collider2D/attack controller; no hit volume was invented. Hit permission/data still require configuration.", effect);
            return;
        }
        controller.InitializeEffect(effectAttackStats, gameObject, settings);
    }
}
