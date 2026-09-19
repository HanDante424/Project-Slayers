using UnityEngine;

public class EnemyHealthBarBinder : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Transform healthBarAnchor;
    [SerializeField] private GameObject healthBarPrefab;

    private EnemyHealthBarUI healthBarUI;

    private void Awake()
    {
        if (health == null ||
            healthBarAnchor == null ||
            healthBarPrefab == null)
        {
            return;
        }

        GameObject healthBarObject = Instantiate(
            healthBarPrefab,
            healthBarAnchor
        );

        healthBarObject.transform.localPosition = Vector3.zero;
        healthBarObject.transform.localRotation = Quaternion.identity;

        healthBarUI =
            healthBarObject.GetComponentInChildren<EnemyHealthBarUI>(true);

        if (healthBarUI != null)
        {
            healthBarUI.Bind(health);
        }
    }
}