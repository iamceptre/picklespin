using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class SpellSpawner : MonoBehaviour
{
    //private Vector3 hiddenPosition = new Vector3(0, -50, 0);
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

    private NewRoundDisplayText newRoundDisplayText;

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
        //Randomize();
        SpellPickupable spawned = spellsLoPool.Get();
        //Debug.Log(spawned.transform.position);
        //spawned.transform.position = spawnPoints[rrrandom].position;
        //Debug.Log(spawnPoints[rrrandom].position + " Should be the same as " + spawned.transform.position);
        spawned.SetOccupiedWaypoint(rrrandom, this, 0);
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
        //itemInstance.transform.position = hiddenPosition;
        return itemInstance;
    }

    private void OnGetFromPool(SpellPickupable pooledItem)
    {
        int maxRange = spawnPoints.Length;
        int minRange = 0;

        rrrandom = Random.Range(minRange, maxRange);

        while (isSpawnPointTaken[rrrandom])
        {
            rrrandom = Random.Range(minRange, maxRange);
        }

        pooledItem.gameObject.SetActive(true);
        pooledItem.transform.position = spawnPoints[rrrandom].position;
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
