using UnityEngine;

// Static scene-to-stage reference for future objective/spawn/reward integration.
// This does not replace CombatPlayerEntry's per-run selection or execute MapData content.
[DisallowMultipleComponent]
public sealed class CombatSceneDefinition : MonoBehaviour
{
    [SerializeField] private MapData stage;
    public MapData Stage => stage;
}
