using UnityEngine;
using UnityEngine.Pool;

public class GoblinPool : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GoblinEnemy goblinPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform player;

    [Header("Pool Settings")]
    [SerializeField] private int defaultCapacity = 1;
    [SerializeField] private int maxPoolSize = 1;

    private ObjectPool<GoblinEnemy> pool;

    private GoblinEnemy activeGoblin;

    private void Awake()
    {
        pool = new ObjectPool<GoblinEnemy>(
            CreateGoblin,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            true,
            defaultCapacity,
            maxPoolSize
        );
    }

    public void SetPlayer(Transform target) => player = target;

    public void SpawnGoblin()
    {
        // 현재 활성화된 고블린이 있다면
        // 추가 소환하지 않는다.
        if (activeGoblin != null &&
            activeGoblin.gameObject.activeSelf)
        {
            return;
        }

        activeGoblin = pool.Get();

        Vector3 spawnPosition =
            spawnPoint != null
                ? spawnPoint.position
                : transform.position;

        activeGoblin.transform.position =
            spawnPosition;

        activeGoblin.Initialize(
            player,
            ReleaseGoblin
        );
    }

    private GoblinEnemy CreateGoblin()
    {
        GoblinEnemy goblin = Instantiate(
            goblinPrefab
        );

        goblin.gameObject.SetActive(false);

        return goblin;
    }

    private void OnTakeFromPool(
        GoblinEnemy goblin)
    {
        goblin.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(
        GoblinEnemy goblin)
    {
        goblin.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(
        GoblinEnemy goblin)
    {
        Destroy(goblin.gameObject);
    }

    private void ReleaseGoblin(
        GoblinEnemy goblin)
    {
        if (activeGoblin == goblin)
        {
            activeGoblin = null;
        }

        pool.Release(goblin);
    }
}
