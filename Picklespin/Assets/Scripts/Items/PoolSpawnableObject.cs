using UnityEngine;
using UnityEngine.Pool;

public class PoolSpawnableObject : MonoBehaviour
{
    private PickableBonusesSpawner pickableBonusesSpawner;
    private int myOccupiedWaypointIndex;
    private ObjectPool<PoolSpawnableObject> _pool;

    private bool released;

    public int WaypointIndex => myOccupiedWaypointIndex;

    private void OnEnable() => released = false;

    public void OccupyPoint(int point, PickableBonusesSpawner spawnerScript)
    {
        pickableBonusesSpawner = spawnerScript;
        myOccupiedWaypointIndex = point;
    }

    public void FreeUpSlot()
    {
        if (released) return;
        released = true;

        pickableBonusesSpawner.ReleasePoint(this, myOccupiedWaypointIndex);

        if (_pool != null) _pool.Release(this);
        else pickableBonusesSpawner.allPotionsPool.Release(this);
    }

    public void SetPool(ObjectPool<PoolSpawnableObject> pool)
    {
        _pool = pool;
    }
}
