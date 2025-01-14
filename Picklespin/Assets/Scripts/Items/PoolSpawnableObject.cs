using UnityEngine;
using UnityEngine.Pool;

public class PoolSpawnableObject : MonoBehaviour
{
    private PickupableBonusesSpawner pickupableBonusesSpawner;
    private int myOccupiedWaypointIndex;
    private ObjectPool<PoolSpawnableObject> _pool;

    public void SetOccupiedWaypoint(int myWaypointIndex, PickupableBonusesSpawner spawnerScript)
    {
        if (pickupableBonusesSpawner == null)
        {
            pickupableBonusesSpawner = spawnerScript;
        }
        myOccupiedWaypointIndex = myWaypointIndex;
        pickupableBonusesSpawner.isSpawnPointTaken[myOccupiedWaypointIndex] = true;
    }

    public void FreeUpSlot()
    {
        pickupableBonusesSpawner.isSpawnPointTaken[myOccupiedWaypointIndex] = false;
        pickupableBonusesSpawner.howManyToSpawn++;
        pickupableBonusesSpawner.howManyToSpawn = Mathf.Clamp(
            pickupableBonusesSpawner.howManyToSpawn,
            0,
            pickupableBonusesSpawner.startingHowManyToSpawn
        );
        pickupableBonusesSpawner.allPotionsPool.Release(this);
        pickupableBonusesSpawner.avaliableSpawnPointsCount++;
        pickupableBonusesSpawner.avaliableSpawnPointsCount = Mathf.Clamp(
            pickupableBonusesSpawner.avaliableSpawnPointsCount,
            0,
            pickupableBonusesSpawner.spawnPoints.Length
        );
    }

    public void SetPool(ObjectPool<PoolSpawnableObject> pool)
    {
        _pool = pool;
    }
}
