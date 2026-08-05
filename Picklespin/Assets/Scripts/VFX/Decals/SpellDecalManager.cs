using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class SpellDecalManager : MonoBehaviour
{
    public static SpellDecalManager Instance { get; private set; }

    [SerializeField] private List<DecalType> decalTypes;

    private readonly Dictionary<SpellDecalType, ObjectPool<SpellDecalDissolve>> decalPools = new();
    private readonly Dictionary<SpellDecalType, System.Action<SpellDecalDissolve>> returnCallbacks = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var decalType in decalTypes)
        {
            if (decalType == null || !decalType.decalPrefab) continue;

            if (decalPools.ContainsKey(decalType.decalType))
            {
                Debug.LogWarning($"{nameof(SpellDecalManager)}: two entries for {decalType.decalType} - keeping the first", this);
                continue;
            }

            var pool = new ObjectPool<SpellDecalDissolve>(
                createFunc: () => CreateDecal(decalType.decalPrefab),
                actionOnGet: (decal) => decal.gameObject.SetActive(true),
                actionOnRelease: (decal) => decal.gameObject.SetActive(false),
                actionOnDestroy: (decal) => Destroy(decal.gameObject),
                collectionCheck: false,
                defaultCapacity: decalType.pooledCount,
                maxSize: decalType.pooledCount * 2
            );

            decalPools.Add(decalType.decalType, pool);
            SpellDecalType type = decalType.decalType;
            returnCallbacks.Add(type, decal => ReturnDecal(type, decal));

            pool.Prewarm(decalType.pooledCount);
        }
    }

    private SpellDecalDissolve CreateDecal(SpellDecalDissolve prefab)
    {
        SpellDecalDissolve decal = Instantiate(prefab, transform);
        return decal;
    }

    public void SpawnDecal(Vector3 position, Quaternion rotation, SpellDecalType type, int hitTag)
    {
        if (!decalPools.TryGetValue(type, out var pool)) return;

        var decal = pool.Get();
        if (decal == null) return;

        decal.transform.SetPositionAndRotation(position, rotation);
        decal.Initialize(returnCallbacks[type], hitTag);
    }

    private void ReturnDecal(SpellDecalType type, SpellDecalDissolve decal)
    {
        if (decalPools.TryGetValue(type, out ObjectPool<SpellDecalDissolve> pool))
        {
            pool.Release(decal);
        }
        else
        {
            Debug.LogWarning($"No decal pool found to return a {type} decal.");
            Destroy(decal.gameObject);
        }
    }
}
