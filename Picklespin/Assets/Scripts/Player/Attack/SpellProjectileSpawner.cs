using UnityEngine;
using UnityEngine.Pool;

public class SpellProjectileSpawner : MonoBehaviour
{
    public static SpellProjectileSpawner instance;

    private Bullet[] bulletPrefab;

    private static readonly int[] PoolSize = { 8, 3, 4 };

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

        bulletPrefab = Attack.instance ? Attack.instance.bulletPrefab : null;
        if (bulletPrefab == null || bulletPrefab.Length == 0)
        {
            DevLog.Error($"{nameof(SpellProjectileSpawner)}: no spells in Attack.bulletPrefab - nothing can be cast.", this);
            return;
        }

        pools = new ObjectPool<Bullet>[bulletPrefab.Length];
        for (int i = 0; i < bulletPrefab.Length; i++) pools[i] = CreatePool((SpellId)i);
        for (int i = 0; i < bulletPrefab.Length; i++) PreInstantiate((SpellId)i);
    }

    public void SpawnSpell(SpellId spell)
    {
        int slot = (int)spell;
        if (pools == null || slot < 0 || slot >= pools.Length)
        {
            DevLog.Warn($"spell spawner has no pool for {spell}", this);
            return;
        }

        Bullet spawned = pools[slot].Get();
        if (spell == SpellId.Light) RetirePreviousLight(spawned);
        spawned.OnShoot();
    }

    private ObjectPool<Bullet> CreatePool(SpellId spell)
    {
        int slot = (int)spell;
        int capacity = CapacityFor(spell);
        ObjectPool<Bullet> pool = null;
        pool = new ObjectPool<Bullet>(
            createFunc: () =>
            {
                Bullet spawned = Instantiate(bulletPrefab[slot]);
                spawned.SetPool(pool);

                spawned.gameObject.SetActive(false);
                return spawned;
            },
            OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
            collectionCheck: false, defaultCapacity: capacity, maxSize: capacity * 2);
        return pool;
    }

    private void PreInstantiate(SpellId spell)
    {
        if (spell == SpellId.Light) return;

        int slot = (int)spell;
        int capacity = CapacityFor(spell);
        var warmed = new Bullet[capacity];
        for (int i = 0; i < capacity; i++) warmed[i] = pools[slot].Get();
        for (int i = 0; i < capacity; i++) pools[slot].Release(warmed[i]);
    }

    private int CapacityFor(SpellId spell) => (int)spell < PoolSize.Length ? PoolSize[(int)spell] : 4;

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
        if (previousLightSpell != null && previousLightSpell != newest) previousLightSpell.Retire();
        previousLightSpell = newest;
    }
}
