using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PickableBonusesSpawner : MonoBehaviour
{
    public static PickableBonusesSpawner instance { get; private set; }

    [Header("Available Bonuses")]
    [SerializeField] private PoolSpawnableObject[] bonuses;
    [SerializeField, Tooltip("the one potion Umbral sees - it replaces every bonus above while the class is held")]
    private PoolSpawnableObject umbralPotion;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    public ObjectPool<PoolSpawnableObject> allPotionsPool;
    private ObjectPool<PoolSpawnableObject> umbralPool;

    private SpawnPoints points;

    private readonly Vector3 initialSpawnPosition = new(0, -50, 0);
    private readonly WaitForSeconds scatterTime = new(0.05f);
    private readonly List<PoolSpawnableObject> live = new();
    private readonly List<PoolSpawnableObject> swapBuffer = new();

    private Coroutine currentSpawnRoutine;
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
        points = new SpawnPoints(spawnPoints);
    }

    private void Start()
    {
        allPotionsPool = CreatePool(CreateItem);
        allPotionsPool.Prewarm(spawnPoints.Length);

        if (umbralPotion)
        {
            umbralPool = CreatePool(CreateUmbralItem);
            umbralPool.Prewarm(spawnPoints.Length);
        }
        else
        {
            DevLog.Warn($"{nameof(PickableBonusesSpawner)}: no umbral potion assigned - Umbral keeps the normal bonuses", this);
        }
    }

    private void OnEnable() => PlayerClasses.Changed += SwapLivePotions;

    private void OnDisable() => PlayerClasses.Changed -= SwapLivePotions;

    private ObjectPool<PoolSpawnableObject> CreatePool(Func<PoolSpawnableObject> create)
    {
        return new ObjectPool<PoolSpawnableObject>(
            create,
            item => item.gameObject.SetActive(true),
            item =>
            {
                item.gameObject.SetActive(false);
                item.transform.position = initialSpawnPosition;
            },
            item => Destroy(item.gameObject),
            true,
            spawnPoints.Length,
            spawnPoints.Length * 2);
    }

    public void SpawnBonuses(int howManyToSpawn)
    {
        if (currentSpawnRoutine != null) StopCoroutine(currentSpawnRoutine);

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

            if (!points.TryReserve(out int point, pointIsUsable)) break;

            PoolSpawnableObject item = take();

            if (!item)
            {
                points.Release(point);
                break;
            }

            Place(item, point);
        }

        currentSpawnRoutine = null;
    }

    private void Place(PoolSpawnableObject item, int point)
    {
        points.Reserve(point);
        item.transform.position = points.PositionOf(point);
        item.OccupyPoint(point, this);
        live.Add(item);

        if (item.TryGetComponent(out PickableItem pickable)) pickable.StartFloating();
    }

    public void ReleasePoint(PoolSpawnableObject item, int point)
    {
        live.Remove(item);
        points.Release(point);
    }

    private void SwapLivePotions()
    {
        if (UmbralActive == liveIsUmbral) return;
        liveIsUmbral = UmbralActive;

        if (live.Count == 0) return;

        swapBuffer.Clear();
        swapBuffer.AddRange(live);

        foreach (PoolSpawnableObject taken in swapBuffer)
        {
            int point = taken.WaypointIndex;
            taken.FreeUpSlot();

            PoolSpawnableObject fresh = TakePotion();

            if (!fresh) continue;

            Place(fresh, point);
        }
    }

    private PoolSpawnableObject CreateItem()
    {
        PoolSpawnableObject itemInstance = Instantiate(bonuses[UnityEngine.Random.Range(0, bonuses.Length)], initialSpawnPosition, Quaternion.identity);
        itemInstance.SetPool(allPotionsPool);
        return itemInstance;
    }

    private PoolSpawnableObject CreateUmbralItem()
    {
        PoolSpawnableObject itemInstance = Instantiate(umbralPotion, initialSpawnPosition, Quaternion.identity);
        itemInstance.SetPool(umbralPool);
        return itemInstance;
    }
}
