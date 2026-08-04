using UnityEngine;
using UnityEngine.Pool;

public class DamageUISpawner : MonoBehaviour
{
    public static DamageUISpawner instance { get; private set; }

    [SerializeField] private DamageUIV2 damageUi;

    private const int PoolCapacity = 16;

    private ObjectPool<DamageUIV2> pool;
    private readonly Vector3 offset = new Vector3(0, 4, 0);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        pool = new ObjectPool<DamageUIV2>(CreateItem, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, PoolCapacity, PoolCapacity * 4);
    }

    private void Start()
    {
        pool.Prewarm(PoolCapacity);
    }

    public void Spawn(Vector3 whereIshouldGo, int howMuchDamageDealt, bool isCritical)
    {
        whereIshouldGo += Random.insideUnitSphere + offset;
        DamageUIV2 spawned = pool.Get();
        spawned.Do(whereIshouldGo, howMuchDamageDealt, isCritical);
    }

    private DamageUIV2 CreateItem()
    {
        DamageUIV2 item = Instantiate(damageUi, transform);
        item.SetPool(pool);
        return item;
    }

    private void OnGetFromPool(DamageUIV2 item) => item.gameObject.SetActive(true);
    private void OnReleaseToPool(DamageUIV2 item) => item.gameObject.SetActive(false);
    private void OnDestroyPooledObject(DamageUIV2 item) => Destroy(item.gameObject);
}
