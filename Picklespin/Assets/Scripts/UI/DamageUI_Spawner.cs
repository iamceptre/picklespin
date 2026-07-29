using UnityEngine;
using UnityEngine.Pool;

public class DamageUI_Spawner : MonoBehaviour
{
    public static DamageUI_Spawner instance { get; private set; }

    [SerializeField] private DamageUI_V2 damageUi;

    private ObjectPool<DamageUI_V2> pool;
    private readonly Vector3 offset = new Vector3(0, 4, 0);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
        pool = new ObjectPool<DamageUI_V2>(CreateItem, OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, false, 16, 64);
    }

    public void Spawn(Vector3 whereIshouldGo, int howMuchDamageDealt, bool isCritical)
    {
        whereIshouldGo += Random.insideUnitSphere + offset;
        DamageUI_V2 spawned = pool.Get();
        spawned.Do(whereIshouldGo, howMuchDamageDealt, isCritical);
    }

    private DamageUI_V2 CreateItem()
    {
        DamageUI_V2 item = Instantiate(damageUi, transform);
        item.SetPool(pool);
        return item;
    }

    private void OnGetFromPool(DamageUI_V2 item) => item.gameObject.SetActive(true);
    private void OnReleaseToPool(DamageUI_V2 item) => item.gameObject.SetActive(false);
    private void OnDestroyPooledObject(DamageUI_V2 item) => Destroy(item.gameObject);
}
