using System;
using System.Collections;
using System.Collections.Generic;
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

    private Coroutine currentSpawnRoutine;

    private readonly WaitForSeconds scatterTime = new(0.05f);
    private readonly List<int> freePointBuffer = new();

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
        currentSpawnRoutine = ScatterSpawn(allPotionsPool.Get, howManyToSpawn);
    }

    // The one way anything lands on the map: a free point at random, one item per
    // scatterTime so they arrive one after another instead of all in one frame.
    // Callers differ only in which pool the item comes from and, optionally, which
    // points they will accept at all (the mercy drop takes only unseen ones).
    public Coroutine ScatterSpawn(Func<PoolSpawnableObject> take, int count, Func<Vector3, bool> pointIsUsable = null)
    {
        return StartCoroutine(ScatterRoutine(take, count, pointIsUsable));
    }

    private IEnumerator ScatterRoutine(Func<PoolSpawnableObject> take, int count, Func<Vector3, bool> pointIsUsable)
    {
        for (int i = 0; i < count; i++)
        {
            yield return scatterTime;

            // the point is chosen before the item is taken, so a spawn that cannot be
            // placed never strands one outside its pool
            int index = PickFreePoint(pointIsUsable);
            if (index < 0) break;

            PoolSpawnableObject item = take();
            if (!item) break;

            item.transform.position = spawnPoints[index].position;
            item.SetOccupiedWaypoint(index, this);
            avaliableSpawnPointsCount = Mathf.Max(0, avaliableSpawnPointsCount - 1);
            if (item.TryGetComponent(out Pickupable_Item pickupable)) pickupable.StartFloating();
        }

        currentSpawnRoutine = null;
    }

    private int PickFreePoint(Func<Vector3, bool> pointIsUsable)
    {
        freePointBuffer.Clear();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (isSpawnPointTaken[i] || !spawnPoints[i]) continue;
            if (pointIsUsable != null && !pointIsUsable(spawnPoints[i].position)) continue;
            freePointBuffer.Add(i);
        }

        return freePointBuffer.Count == 0 ? -1 : freePointBuffer[UnityEngine.Random.Range(0, freePointBuffer.Count)];
    }

    private PoolSpawnableObject CreateItem()
    {
        var prefab = bonuses[UnityEngine.Random.Range(0, bonuses.Length)];
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

}
