using UnityEngine;

// CombatPlayerEntry (-1000) follows first; confinement only adjusts the resulting camera position.
[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class OrthographicCameraBounds : MonoBehaviour
{
    [Tooltip("Axis-aligned world rectangle. Only box size/offset/transform are used, so the collider and its GameObject may remain disabled (no physics contacts).")]
    [SerializeField] private BoxCollider2D cameraBounds;
    private Camera targetCamera;

    private void Awake() => targetCamera = GetComponent<Camera>();
    private void OnEnable() => ConstrainPosition();
    private void LateUpdate() => ConstrainPosition();

    public void ConstrainPosition()
    {
        if (targetCamera == null || !targetCamera.orthographic || cameraBounds == null) return;
        Bounds bounds = GetWorldBounds();
        float halfHeight = targetCamera.orthographicSize;
        float halfWidth = halfHeight * targetCamera.aspect;
        Vector3 position = transform.position;
        position.x = ConstrainAxis(position.x, bounds.min.x, bounds.max.x, halfWidth);
        position.y = ConstrainAxis(position.y, bounds.min.y, bounds.max.y, halfHeight);
        transform.position = position;
    }

    private static float ConstrainAxis(float position, float min, float max, float halfSize)
        => max - min <= halfSize * 2f ? (min + max) * 0.5f : Mathf.Clamp(position, min + halfSize, max - halfSize);

    // Collider2D.bounds is empty for inactive/disabled colliders. Read authored geometry instead,
    // including live Inspector changes, without needing a physics transform sync or a live trigger.
    public Bounds GetWorldBounds()
    {
        if (cameraBounds == null) return default;
        Transform area = cameraBounds.transform;
        Vector2 half = cameraBounds.size * 0.5f;
        Vector2 center = cameraBounds.offset;
        var bounds = new Bounds(area.TransformPoint(center - half), Vector3.zero);
        bounds.Encapsulate(area.TransformPoint(center + half));
        bounds.Encapsulate(area.TransformPoint(center + new Vector2(half.x, -half.y)));
        bounds.Encapsulate(area.TransformPoint(center + new Vector2(-half.x, half.y)));
        return bounds;
    }

    private void OnDrawGizmos()
    {
        if (cameraBounds == null) return;
        Color previous = Gizmos.color;
        Gizmos.color = Color.green;
        Bounds bounds = GetWorldBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = previous;
    }
}
