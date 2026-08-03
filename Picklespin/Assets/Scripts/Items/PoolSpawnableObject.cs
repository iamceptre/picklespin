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

    public void SetOccupiedWaypoint(int myWaypointIndex, PickableBonusesSpawner spawnerScript)
    {
        if (pickableBonusesSpawner == null)
        {
            pickableBonusesSpawner = spawnerScript;
        }
        myOccupiedWaypointIndex = myWaypointIndex;
        pickableBonusesSpawner.isSpawnPointTaken[myOccupiedWaypointIndex] = true;
    }

    public void FreeUpSlot()
    {
        if (released) return;
        released = true;

        pickableBonusesSpawner.Forget(this);
        pickableBonusesSpawner.isSpawnPointTaken[myOccupiedWaypointIndex] = false;
        pickableBonusesSpawner.howManyToSpawn++;
        pickableBonusesSpawner.howManyToSpawn = Mathf.Clamp(
            pickableBonusesSpawner.howManyToSpawn,
            0,
            pickableBonusesSpawner.startingHowManyToSpawn
        );

        if (_pool != null) _pool.Release(this);
        else pickableBonusesSpawner.allPotionsPool.Release(this);
        pickableBonusesSpawner.avaliableSpawnPointsCount++;
        pickableBonusesSpawner.avaliableSpawnPointsCount = Mathf.Clamp(
            pickableBonusesSpawner.avaliableSpawnPointsCount,
            0,
            pickableBonusesSpawner.spawnPoints.Length
        );
    }

    public void SetPool(ObjectPool<PoolSpawnableObject> pool)
    {
        _pool = pool;
    }
}
