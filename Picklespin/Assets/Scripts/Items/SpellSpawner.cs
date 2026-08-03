using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class SpellSpawner : MonoBehaviour
{
    public static SpellSpawner instance;

    public int howManyToSpawn;
    private int startingHowManyToSpawn;

    [SerializeField] private SpellPickable[] spellsLo;

    public Transform[] spawnPoints;
    [HideInInspector] public bool[] isSpawnPointTaken;
    [HideInInspector] public int avaliableSpawnPointsCount;

    public ObjectPool<SpellPickable> spellsLoPool;

    private int rrrandom;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        if (howManyToSpawn > spawnPoints.Length)
        {
            howManyToSpawn = spawnPoints.Length;
        }

        isSpawnPointTaken = new bool[spawnPoints.Length];
        avaliableSpawnPointsCount = spawnPoints.Length;
        startingHowManyToSpawn = howManyToSpawn;
    }

    private void Start()
    {
        spellsLoPool = new ObjectPool<SpellPickable>(CreateItem, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, spawnPoints.Length, spawnPoints.Length * 2);
        PreInstantiate();
    }

    private void PreInstantiate()
    {
        var tempList = new SpellPickable[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            tempList[i] = spellsLoPool.Get();
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spellsLoPool.Release(tempList[i]);
        }
    }

    public void SpawnSpellsLo(int howManyToSpawn)
    {
        StartCoroutine(SpawnRoutine(howManyToSpawn));
    }

    private IEnumerator SpawnRoutine(int howManyToSpawn)
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            yield return new WaitForSeconds(i * 0.1f);
            SpawnLo();
        }
        avaliableSpawnPointsCount -= howManyToSpawn;
        ClampSpawnCount();
    }

    private void SpawnLo()
    {
        SpellPickable spawned = spellsLoPool.Get();
        spawned.SetOccupiedWaypoint(rrrandom, this);
    }

    private SpellPickable CreateItem()
    {
        SpellPickable itemInstance = Instantiate(spellsLo[Random.Range(0, spellsLo.Length)]);
        itemInstance.SetPool(spellsLoPool);
        return itemInstance;
    }

    private void OnGetFromPool(SpellPickable pooledItem)
    {
        int maxRange = spawnPoints.Length;
        int minRange = 0;

        if (avaliableSpawnPointsCount <= 0)
        {
            DevLog.Warn("No available spawn points.");
            return;
        }

        do
        {
            rrrandom = Random.Range(minRange, maxRange);
        }
        while (isSpawnPointTaken[rrrandom]);

        pooledItem.gameObject.SetActive(true);
        pooledItem.transform.position = spawnPoints[rrrandom].position;
    }

    private void OnReleaseToPool(SpellPickable pooledItem)
    {
        pooledItem.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(SpellPickable pooledItem)
    {
        Destroy(pooledItem.gameObject);
    }

    public void ClampSpawnCount()
    {
        howManyToSpawn = startingHowManyToSpawn;
        howManyToSpawn = Mathf.Clamp(howManyToSpawn, 0, avaliableSpawnPointsCount);
    }
}