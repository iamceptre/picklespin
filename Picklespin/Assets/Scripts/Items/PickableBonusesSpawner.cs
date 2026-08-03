using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PickableBonusesSpawner : MonoBehaviour
{
    [Header("Singleton")]
    public static PickableBonusesSpawner instance { get; private set; }

    [Header("Spawn Settings")]
    public int howManyToSpawn;
    [HideInInspector] public int startingHowManyToSpawn;

    [Header("Available Bonuses")]
    [SerializeField] private PoolSpawnableObject[] bonuses;
    [SerializeField, Tooltip("the one potion Umbral sees - it replaces every bonus above while the class is held")]
    private PoolSpawnableObject umbralPotion;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    [HideInInspector] public bool[] isSpawnPointTaken;
    [HideInInspector] public int avaliableSpawnPointsCount;

    [Header("Object Pool")]
    public ObjectPool<PoolSpawnableObject> allPotionsPool;
    private ObjectPool<PoolSpawnableObject> umbralPool;

    [Header("Instantiation")]
    private readonly Vector3 initialSpawnPosition = new(0, -50, 0);

    private Coroutine currentSpawnRoutine;

    private readonly WaitForSeconds scatterTime = new(0.05f);
    private readonly List<int> freePointBuffer = new();
    private readonly List<PoolSpawnableObject> live = new();
    private readonly List<PoolSpawnableObject> swapBuffer = new();
    private bool liveIsUmbral;

    private static bool UmbralActive => PlayerClasses.Chosen == PlayerClassId.Umbral;

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
        allPotionsPool = CreatePool(CreateItem);
        if (umbralPotion) umbralPool = CreatePool(CreateUmbralItem);
        else DevLog.Warn($"{nameof(PickableBonusesSpawner)}: no umbral potion assigned - Umbral keeps the normal bonuses", this);

        PreInstantiate();
    }

    private void OnEnable() => PlayerClasses.Changed += SwapLivePotions;

    private void OnDisable() => PlayerClasses.Changed -= SwapLivePotions;

    private ObjectPool<PoolSpawnableObject> CreatePool(Func<PoolSpawnableObject> create)
    {
        return new ObjectPool<PoolSpawnableObject>(
            create,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooledObject,
            true,
            spawnPoints.Length,
            spawnPoints.Length * 2
        );
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
        currentSpawnRoutine = ScatterSpawn(TakePotion, howManyToSpawn);
    }

    public PoolSpawnableObject TakePotion() => UmbralActive && umbralPool != null ? umbralPool.Get() : allPotionsPool.Get();

    public Coroutine ScatterSpawn(Func<PoolSpawnableObject> take, int count, Func<Vector3, bool> pointIsUsable = null)
    {
        return StartCoroutine(ScatterRoutine(take, count, pointIsUsable));
    }

    private IEnumerator ScatterRoutine(Func<PoolSpawnableObject> take, int count, Func<Vector3, bool> pointIsUsable)
    {
        for (int i = 0; i < count; i++)
        {
            yield return scatterTime;

            int index = PickFreePoint(pointIsUsable);
            if (index < 0) break;

            PoolSpawnableObject item = take();
            if (!item) break;

            Place(item, index);
        }

        currentSpawnRoutine = null;
    }

    private void Place(PoolSpawnableObject item, int index)
    {
        item.transform.position = spawnPoints[index].position;
        item.SetOccupiedWaypoint(index, this);
        avaliableSpawnPointsCount = Mathf.Max(0, avaliableSpawnPointsCount - 1);
        live.Add(item);
        if (item.TryGetComponent(out PickableItem pickable)) pickable.StartFloating();
    }

    public void Forget(PoolSpawnableObject item) => live.Remove(item);

    private void SwapLivePotions()
    {
        if (UmbralActive == liveIsUmbral) return;
        liveIsUmbral = UmbralActive;

        if (live.Count == 0) return;

        swapBuffer.Clear();
        swapBuffer.AddRange(live);
        int spawnBudget = howManyToSpawn;

        foreach (PoolSpawnableObject taken in swapBuffer)
        {
            int index = taken.WaypointIndex;
            taken.FreeUpSlot();

            PoolSpawnableObject fresh = TakePotion();
            if (!fresh) continue;

            Place(fresh, index);
        }

        howManyToSpawn = spawnBudget;
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

    private PoolSpawnableObject CreateUmbralItem()
    {
        var itemInstance = Instantiate(umbralPotion, initialSpawnPosition, Quaternion.identity);
        itemInstance.SetPool(umbralPool);
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
