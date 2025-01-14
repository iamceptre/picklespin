using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PickupableBonusesSpawner : MonoBehaviour
{
    [Header("Singleton")]
    public static PickupableBonusesSpawner instance { get; private set; }

    [Header("Spawn Settings")]
    public int howManyToSpawn;
    [HideInInspector] public int startingHowManyToSpawn;

    [Header("Available Bonuses")]
    [SerializeField] private PoolSpawnableObject[] bonuses;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    [HideInInspector] public bool[] isSpawnPointTaken;
    [HideInInspector] public int avaliableSpawnPointsCount;

    [Header("Object Pool")]
    public ObjectPool<PoolSpawnableObject> allPotionsPool;

    [Header("Instantiation")]
    private readonly Vector3 initialSpawnPosition = new(0, -50, 0);

    private int randomIndex;
    private Coroutine currentSpawnRoutine;

    private readonly WaitForSeconds scatterTime = new(0.05f);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (howManyToSpawn >= spawnPoints.Length)
        {
            howManyToSpawn = spawnPoints.Length - 1;
            if (howManyToSpawn < 0) howManyToSpawn = 0;
        }

        startingHowManyToSpawn = howManyToSpawn;
        isSpawnPointTaken = new bool[spawnPoints.Length];
        avaliableSpawnPointsCount = spawnPoints.Length;
    }

    private void Start()
    {
        allPotionsPool = new ObjectPool<PoolSpawnableObject>(
            CreateItem,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            false,
            spawnPoints.Length,
            spawnPoints.Length * 2
        );
        PreInstantiate();
    }

    private void PreInstantiate()
    {
        var tempList = new PoolSpawnableObject[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            tempList[i] = allPotionsPool.Get();
        }
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            allPotionsPool.Release(tempList[i]);
        }
    }

    public void SpawnBonuses(int howManyToSpawn)
    {
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
        }
        if (howManyToSpawn >= spawnPoints.Length)
        {
            howManyToSpawn = spawnPoints.Length - 1;
            if (howManyToSpawn < 0) howManyToSpawn = 0;
        }
        currentSpawnRoutine = StartCoroutine(SpawnRoutine(howManyToSpawn));
    }

    private IEnumerator SpawnRoutine(int howManyToSpawn)
    {
        int totalToSpawn = Mathf.Min(howManyToSpawn, avaliableSpawnPointsCount);
        int potionsSpawned = 0;

        for (int i = 0; i < totalToSpawn; i++)
        {
            if (avaliableSpawnPointsCount <= 0)
            {
                break;
            }
            yield return scatterTime;
            Spawn();
            potionsSpawned++;
        }

        avaliableSpawnPointsCount -= potionsSpawned;
        currentSpawnRoutine = null;
    }

    private void Spawn()
    {
        Randomize();
        PoolSpawnableObject spawned = allPotionsPool.Get();
        spawned.transform.position = spawnPoints[randomIndex].position;
        spawned.SetOccupiedWaypoint(randomIndex, this);
        spawned.GetComponent<Pickupable_Item>().StartFloating();
    }

    private void Randomize()
    {
        if (avaliableSpawnPointsCount <= 0)
        {
            Debug.LogWarning("No available spawn points.");
            return;
        }
        do
        {
            randomIndex = Random.Range(0, spawnPoints.Length);
        }
        while (isSpawnPointTaken[randomIndex]);
    }

    private PoolSpawnableObject CreateItem()
    {
        var prefab = bonuses[Random.Range(0, bonuses.Length)];
        var itemInstance = Instantiate(prefab, initialSpawnPosition, Quaternion.identity);
        itemInstance.SetPool(allPotionsPool);
        return itemInstance;
    }

    private void OnGetFromPool(PoolSpawnableObject pooledItem)
    {
        pooledItem.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(PoolSpawnableObject pooledItem)
    {
        pooledItem.gameObject.SetActive(false);
        pooledItem.transform.position = initialSpawnPosition;
    }

    private void OnDestroyPooledObject(PoolSpawnableObject pooledItem)
    {
        Destroy(pooledItem.gameObject);
    }

    private void SetSpawnCount(int spawnCount)
    {
        howManyToSpawn = Mathf.Clamp(spawnCount, 0, spawnPoints.Length - 1);
    }
}
