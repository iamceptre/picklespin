using UnityEngine;
using UnityEngine.Pool;

// One pool per spell, indexed by spell ID. The prefab list is deliberately not
// duplicated here: a second copy that drifted would only surface on the first shot.
public class SpellProjectileSpawner : MonoBehaviour
{
    public static SpellProjectileSpawner instance;

    private Bullet[] bulletPrefab;

    // a spell past the end gets the default capacity, so a new one needs no entry
    private static readonly int[] PoolSize = { 8, 3, 4 };

    // the light spell keeps one instance alive, so it must not be warmed up
    private const int LightSpellID = 2;

    private CachedCameraMain cachedCameraMain;
    private Transform spellCastPoint;
    private ObjectPool<Bullet>[] pools;
    private Bullet previousLightSpell;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void Start()
    {
        cachedCameraMain = CachedCameraMain.instance;
        spellCastPoint = cachedCameraMain.cachedTransform;

        // Attack registers the spells; Start, so its Awake has assigned the instance
        bulletPrefab = Attack.instance ? Attack.instance.bulletPrefab : null;
        if (bulletPrefab == null || bulletPrefab.Length == 0)
        {
            Debug.LogError($"{nameof(SpellProjectileSpawner)}: no spells in Attack.bulletPrefab - nothing can be cast.", this);
            return;
        }

        pools = new ObjectPool<Bullet>[bulletPrefab.Length];
        for (int i = 0; i < bulletPrefab.Length; i++) pools[i] = CreatePool(i);
        for (int i = 0; i < bulletPrefab.Length; i++) PreInstantiate(i);
    }

    public void SpawnSpell(int spellID)
    {
        if (pools == null || spellID < 0 || spellID >= pools.Length)
        {
            Debug.LogWarning($"spell spawner has no pool for spell {spellID}", this);
            return;
        }

        Bullet spawned = pools[spellID].Get();
        if (spellID == LightSpellID) RetirePreviousLight(spawned);
        spawned.OnShoot();
    }

    private ObjectPool<Bullet> CreatePool(int spellID)
    {
        int capacity = CapacityFor(spellID);
        ObjectPool<Bullet> pool = null;
        pool = new ObjectPool<Bullet>(
            createFunc: () =>
            {
                Bullet spell = Instantiate(bulletPrefab[spellID]);
                spell.SetPool(pool);
                return spell;
            },
            OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
            collectionCheck: false, defaultCapacity: capacity, maxSize: capacity * 2);
        return pool;
    }

    private void PreInstantiate(int spellID)
    {
        if (spellID == LightSpellID) return;

        int capacity = CapacityFor(spellID);
        var warmed = new Bullet[capacity];
        for (int i = 0; i < capacity; i++) warmed[i] = pools[spellID].Get();
        for (int i = 0; i < capacity; i++) pools[spellID].Release(warmed[i]);
    }

    private int CapacityFor(int spellID) => spellID < PoolSize.Length ? PoolSize[spellID] : 4;

    private void OnGetFromPool(Bullet pooledItem)
    {
        pooledItem.transform.position = spellCastPoint.position;
        pooledItem.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(Bullet pooledItem)
    {
        pooledItem.AfterExplosion();
        pooledItem.gameObject.SetActive(false);
    }

    private void OnDestroyPooledObject(Bullet pooledItem)
    {
        Destroy(pooledItem.gameObject);
    }

    private void RetirePreviousLight(Bullet newest)
    {
        if (previousLightSpell != null) previousLightSpell.ReturnToPool();
        previousLightSpell = newest;
    }
}
