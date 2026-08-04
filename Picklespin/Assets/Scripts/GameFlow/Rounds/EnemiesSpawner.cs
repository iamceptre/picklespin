using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemiesSpawner : MonoBehaviour
{
    public static EnemiesSpawner instance;

    [SerializeField] private GameObject evilEntity;
    [SerializeField] private GameObject evilEntityWhite;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform[] waypointsToPass;

    private const int PrewarmCount = 8;

    private static readonly WaitForSeconds spawnStagger = new(0.2f);
    private int spawnIndex;
    private float spawnPointOffset;

    private ObjectPool<GameObject> easyPool;
    private ObjectPool<GameObject> whitePool;
    private Transform pooledRoot;
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> instanceToPool = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        spawnPointOffset = Random.value; // once per run, so the wave still spreads evenly
        pooledRoot = new GameObject("PooledEnemies").transform;
        pooledRoot.gameObject.SetActive(false); // never active in hierarchy, so nothing on a pooled enemy ever wakes
        easyPool = CreatePool(evilEntity);
        whitePool = CreatePool(evilEntityWhite);
    }

    private void Start()
    {
        easyPool.Prewarm(PrewarmCount);
        whitePool.Prewarm(PrewarmCount);
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        ObjectPool<GameObject> pool = null;
        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject enemy = Instantiate(prefab, pooledRoot); // born under a dead root, so nothing on it wakes yet
                enemy.SetActive(false);
                enemy.transform.SetParent(null, false);
                SilentWarmup.Run(enemy);
                instanceToPool.Add(enemy, pool);
                return enemy;
            },
            actionOnGet: null,      // activation happens in Spawn, after positioning
            actionOnRelease: enemy => enemy.SetActive(false),
            actionOnDestroy: enemy =>
            {
                instanceToPool.Remove(enemy);
                Destroy(enemy);
            },
            collectionCheck: false, defaultCapacity: 8, maxSize: 32);
        return pool;
    }

    public void SpawnEnemiesEasy(int howManyToSpawn)
    {
        StartCoroutine(SpawnWave(easyPool, howManyToSpawn));
    }

    public void SpawnEnemiesWhite(int howManyToSpawn)
    {
        StartCoroutine(SpawnWave(whitePool, howManyToSpawn));
    }

    private IEnumerator SpawnWave(ObjectPool<GameObject> pool, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Spawn(pool);
            yield return spawnStagger;
        }
    }

    private void Spawn(ObjectPool<GameObject> pool)
    {
        Vector2 offset = PhiMath.GoldenSpiralPoint(spawnIndex % 8, 8, 0.5f);
        Transform point = spawnPoints[(int)(PhiMath.GoldenSequence(spawnIndex, spawnPointOffset) * spawnPoints.Length)];
        spawnIndex++;

        GameObject enemy = pool.Get();
        Vector3 spawnPosition = point.position + new Vector3(offset.x, 0, offset.y);
        enemy.transform.position = spawnPosition;

        // the waypoints have to land before ResetAll, which re-shuffles from them
        AiReferences refs = enemy.GetComponentInChildren<AiReferences>(true);
        if (refs)
        {
            if (refs.WaypointsForSpawner) refs.WaypointsForSpawner.cachedPoint = waypointsToPass;
            refs.ResetAll();
        }

        enemy.SetActive(true); // position and state are ready before OnEnable fires

        // sync A*'s internal simulation to the new spot (it remembers the death position otherwise)
        if (enemy.TryGetComponent(out Pathfinding.IAstarAI astarAI)) astarAI.Teleport(spawnPosition);
    }

    public static bool TryDespawn(GameObject enemy)
    {
        if (instance == null || !instance.instanceToPool.TryGetValue(enemy, out ObjectPool<GameObject> pool))
        {
            return false;
        }
        pool.Release(enemy);
        return true;
    }
}
