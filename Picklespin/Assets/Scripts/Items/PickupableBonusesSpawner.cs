using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class PickupableBonusesSpawner : MonoBehaviour
{
    public static PickupableBonusesSpawner instance { get; private set; }

    public int howManyToSpawn;
    [HideInInspector] public int startingHowManyToSpawn;

    [SerializeField] private PoolSpawnableObject[] bonuses;

    public Transform[] spawnPoints;
    [HideInInspector] public bool[] isSpawnPointTaken;
    [HideInInspector] public int avaliableSpawnPointsCount;


    private int rrrandom;

    public ObjectPool<PoolSpawnableObject> allPotionsPool;

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

        startingHowManyToSpawn = howManyToSpawn;
        isSpawnPointTaken = new bool[spawnPoints.Length];
        avaliableSpawnPointsCount = spawnPoints.Length;
    }

    private void Start()
    {
        allPotionsPool = new ObjectPool<PoolSpawnableObject>(CreateItem, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, spawnPoints.Length, spawnPoints.Length * 2);
        PreInstantiate();
    }


    private void PreInstantiate()
    {
                                      //spawnPoints.Length is max object pool count, change it when you do separate pools for every potion
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
        SetSpawnCount(howManyToSpawn);
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            yield return new WaitForSeconds(i * 0.02f);
            Spawn();
        }

        avaliableSpawnPointsCount -= howManyToSpawn;
        howManyToSpawn = Mathf.Clamp(howManyToSpawn, 0, avaliableSpawnPointsCount);
    }

    private void Spawn()
    {
        Randomize();
        PoolSpawnableObject spawned = allPotionsPool.Get();
        spawned.transform.position = spawnPoints[rrrandom].position;
        spawned.SetOccupiedWaypoint(rrrandom, this);
        spawned.GetComponent<Pickupable_Item>().StartFloating();
    }

    private void Randomize()
    {
        int maxRange = spawnPoints.Length;
        int minRange = 0;

        while (isSpawnPointTaken[rrrandom])
        {
            rrrandom = Random.Range(minRange, maxRange);
        }
    }






    private PoolSpawnableObject CreateItem()
    {
        PoolSpawnableObject itemInstance = Instantiate(bonuses[Random.Range(0, bonuses.Length)]); //split every potion into separate pools so you can do weighted random
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
    }

    private void OnDestroyPooledObject(PoolSpawnableObject pooledItem)
    {
        Destroy(pooledItem.gameObject);
    }


    private void SetSpawnCount(int spawnCount) //use it from event system gui, before spawning
    {
        howManyToSpawn = spawnCount;
        howManyToSpawn = Mathf.Clamp(howManyToSpawn, 0, spawnPoints.Length);
    }

}
