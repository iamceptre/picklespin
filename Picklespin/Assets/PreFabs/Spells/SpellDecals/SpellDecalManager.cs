using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class SpellDecalManager : MonoBehaviour
{
    public static SpellDecalManager Instance { get; private set; }

    [SerializeField] private List<DecalType> decalTypes;

    private Dictionary<int, ObjectPool<SpellDecalDissolve>> decalPools = new Dictionary<int, ObjectPool<SpellDecalDissolve>>();

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
            var pool = new ObjectPool<SpellDecalDissolve>(
                createFunc: () => CreateDecal(decalType.decalPrefab),
                actionOnGet: (decal) => decal.gameObject.SetActive(true),
                actionOnRelease: (decal) => decal.gameObject.SetActive(false),
                actionOnDestroy: (decal) => Destroy(decal.gameObject),
                collectionCheck: false,
                defaultCapacity: decalType.pooledCount,
                maxSize: decalType.pooledCount * 2
            );

            decalPools.Add(decalType.spellID, pool);

            PreInstantiateDecals(decalType.spellID, pool, decalType.pooledCount);
        }
    }

    private SpellDecalDissolve CreateDecal(SpellDecalDissolve prefab)
    {
        SpellDecalDissolve decal = Instantiate(prefab, transform);
        return decal;
    }

    private void PreInstantiateDecals(int spellID, ObjectPool<SpellDecalDissolve> pool, int count)
    {
        var tempList = new SpellDecalDissolve[count];

        for (int i = 0; i < count; i++)
        {
            tempList[i] = pool.Get();
        }

        for (int i = 0; i < count; i++)
        {
            pool.Release(tempList[i]);
        }
    }

    public void SpawnDecal(Vector3 position, Quaternion rotation, int spellID, int hitTag)
    {
        if (!decalPools.TryGetValue(spellID, out var pool)) return;

        var decal = pool.Get();
        if (decal == null) return;

        decal.transform.SetPositionAndRotation(position, rotation);
        decal.Initialize((d) => ReturnDecal(spellID, d), hitTag);
    }

    private void ReturnDecal(int spellID, SpellDecalDissolve decal)
    {
        if (decalPools.TryGetValue(spellID, out ObjectPool<SpellDecalDissolve> pool))
        {
            pool.Release(decal);
        }
        else
        {
            Debug.LogWarning($"No decal pool found to return decal for spellID {spellID}.");
            Destroy(decal.gameObject);
        }
    }
}
