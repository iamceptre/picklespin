using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class SpellSpawner : MonoBehaviour
{
    //[SerializeField] private GameObject[] spellsHi;
    public static SpellSpawner instance;

    public int howManyToSpawn;
    private int startingHowManyToSpawn;

    [SerializeField] private SpellPickupable[] spellsLo;

    public Transform[] spawnPoints;
    [HideInInspector] public bool[] isSpawnPointTaken;
    [HideInInspector] public int avaliableSpawnPointsCount;

    public ObjectPool<SpellPickupable> spellsLoPool;

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
        spellsLoPool = new ObjectPool<SpellPickupable>(CreateItem, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, spawnPoints.Length, spawnPoints.Length * 2);
        PreInstantiate();
    }


    private void PreInstantiate()
    {
        //spawnPoints.Length is max object pool count, change it when you do separate pools for every potion
        var tempList = new SpellPickupable[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            tempList[i] = spellsLoPool.Get();
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spellsLoPool.Release(tempList[i]);
        }
    }


    public void SpawnSpellsLo()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
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
        Randomize();
        SpellPickupable spawned = spellsLoPool.Get();
        spawned.transform.position = spawnPoints[rrrandom].position;
        spawned.SetOccupiedWaypoint(rrrandom, this, 0);
    }



    public void SpawnLastSpell()
    {
        Debug.Log("zongi! tyœ wygra³ w tê grê :-D");
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

    public void SetSpawnCount(int spawnCount)
    {
        if (howManyToSpawn >= spawnPoints.Length)
        {
            howManyToSpawn = spawnPoints.Length;
        }
        else
        {
            howManyToSpawn = spawnCount;
        }
    }

    private SpellPickupable CreateItem()
    {
        SpellPickupable itemInstance = Instantiate(spellsLo[Random.Range(0, spellsLo.Length)]); 
        itemInstance.SetPool(spellsLoPool);
        return itemInstance;
    }

    private void OnGetFromPool(SpellPickupable pooledItem)
    {
        pooledItem.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(SpellPickupable pooledItem)
    {
        pooledItem.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(SpellPickupable pooledItem)
    {
        Destroy(pooledItem.gameObject);
    }

    public void ClampSpawnCount()
    {
        howManyToSpawn = startingHowManyToSpawn;
        howManyToSpawn = Mathf.Clamp(howManyToSpawn, 0, avaliableSpawnPointsCount);
    }
}
