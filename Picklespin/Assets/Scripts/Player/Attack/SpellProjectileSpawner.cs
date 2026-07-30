using UnityEngine;
using UnityEngine.Pool;

// One pool per spell, indexed by spell ID so bulletPrefab, PoolSize and the
// pools all line up. Adding a spell is a prefab in the array plus a size here —
// it used to be a create function, a pre-instantiate function and a switch case
// each, all copies of one another.
public class SpellProjectileSpawner : MonoBehaviour
{
    public static SpellProjectileSpawner instance;

    [SerializeField] private Bullet[] bulletPrefab;

    // per spell ID: purple, fireball, light
    private static readonly int[] PoolSize = { 8, 3, 4 };

    // the light spell keeps a single instance alive and retires it itself
    // (OffPreviousLights), so it must not be warmed up by activating spares
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

        pools = new ObjectPool<Bullet>[bulletPrefab.Length];
        for (int i = 0; i < bulletPrefab.Length; i++) pools[i] = CreatePool(i);
        for (int i = 0; i < bulletPrefab.Length; i++) PreInstantiate(i);
    }

    public void SpawnSpell(int spellID) //Actually spawns the projectile in the world
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
                spell.SetPool(pool); // assigned after construction, read only when the pool runs it
                return spell;
            },
            OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
            collectionCheck: false, defaultCapacity: capacity, maxSize: capacity * 2);
        return pool;
    }

    // fills the pool up front so the first cast of a spell never pays for an Instantiate
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

    // only one light spell burns at a time: casting a new one retires the last
    private void RetirePreviousLight(Bullet newest)
    {
        if (previousLightSpell != null) previousLightSpell.ReturnToPool();
        previousLightSpell = newest;
    }
}
