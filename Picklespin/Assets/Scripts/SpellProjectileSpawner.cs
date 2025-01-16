using UnityEngine;
using UnityEngine.Pool;

public class SpellProjectileSpawner : MonoBehaviour
{
    public static SpellProjectileSpawner instance;

    private RecoilMultiplier recoilMultiplier;
    private CachedCameraMain cachedCameraMain;
    private Transform spellCastPoint;


    [SerializeField] private Bullet[] bulletPrefab;


    [Header("Pooling")]

    [Header("Purple")]
    private ObjectPool<Bullet> purpleSpellPool;
    private readonly int pooledPurpleSpellsCount = 8;

    [Header("Fireball")]
    private ObjectPool<Bullet> fireballSpellPool;
    private readonly int pooledFireballSpellsCount = 3;

    [Header("Light")]
    private ObjectPool<Bullet> lightSpellPool;
    private readonly int pooledlightSpellsCount = 4;
    private Bullet previousLightSpell;

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

    }
    void Start()
    {
        cachedCameraMain = CachedCameraMain.instance;
        recoilMultiplier = RecoilMultiplier.instance;
        spellCastPoint = cachedCameraMain.cachedTransform;

        purpleSpellPool = new ObjectPool<Bullet>(CreatePurple, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, pooledPurpleSpellsCount, pooledPurpleSpellsCount * 2);
        fireballSpellPool = new ObjectPool<Bullet>(CreateFireball, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, pooledFireballSpellsCount, pooledFireballSpellsCount * 2);
        lightSpellPool = new ObjectPool<Bullet>(CreateLight, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, pooledlightSpellsCount, pooledlightSpellsCount * 2);
        PreInstantiate();
    }

    private void PreInstantiate()
    {
        PreInstantiatePurple();
        PreInstantiateFireball();
       // PreInstantiateLight();
    }



    public void SpawnSpell(int spellID) //Actually spawns the projectile in the world
    {
        switch (spellID)
        {
            case 0:
                Bullet purpleSpawned = purpleSpellPool.Get();
                purpleSpawned.OnShoot();
                break;

            case 1:
                Bullet fireballSpawned = fireballSpellPool.Get();
                fireballSpawned.OnShoot();
                break;

            case 2:
                Bullet lightSpawned = lightSpellPool.Get();
                OffPreviousLights(lightSpawned);
                lightSpawned.OnShoot();
                break;

            default:
                Debug.Log("spell spawner not set");
                break;
        }

    }




    private void PreInstantiatePurple()
    {
        var tempList = new Bullet[pooledPurpleSpellsCount];

        for (int i = 0; i < pooledPurpleSpellsCount; i++)
        {
            tempList[i] = purpleSpellPool.Get();
        }

        for (int i = 0; i < pooledPurpleSpellsCount; i++)
        {
            purpleSpellPool.Release(tempList[i]);
        }
    }

    private void PreInstantiateFireball()
    {
        var tempList = new Bullet[pooledFireballSpellsCount];

        for (int i = 0; i < pooledFireballSpellsCount; i++)
        {
            tempList[i] = fireballSpellPool.Get();
        }

        for (int i = 0; i < pooledFireballSpellsCount; i++)
        {
            fireballSpellPool.Release(tempList[i]);
        }
    }

    private void PreInstantiateLight()
    {
        var tempList = new Bullet[pooledlightSpellsCount];

        for (int i = 0; i < pooledlightSpellsCount; i++)
        {
            tempList[i] = lightSpellPool.Get();
        }

        for (int i = 0; i < pooledlightSpellsCount; i++)
        {
            lightSpellPool.Release(tempList[i]);
        }
    }


    private Bullet CreatePurple()
    {
        Bullet purpleSpellInstance = Instantiate(bulletPrefab[0]);
        purpleSpellInstance.SetPool(purpleSpellPool);
        return purpleSpellInstance;
    }

    private Bullet CreateFireball()
    {
        Bullet fireballInstance = Instantiate(bulletPrefab[1]);
        fireballInstance.SetPool(fireballSpellPool);
        return fireballInstance;
    }
    private Bullet CreateLight()
    {
        Bullet lightInstance = Instantiate(bulletPrefab[2]);
        lightInstance.SetPool(lightSpellPool);
        return lightInstance;
    }

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


    private void OffPreviousLights(Bullet previousOne)
    {
        if (previousLightSpell != null)
        {
            previousLightSpell.ReturnToPool();
        }

        previousLightSpell = previousOne;
    }


}
