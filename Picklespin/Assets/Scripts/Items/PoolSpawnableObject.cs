using UnityEngine;
using UnityEngine.Pool;

public class PoolSpawnableObject : MonoBehaviour
{
    private PickupableBonusesSpawner pickupableBonusesSpawner;
    private SpellSpawner spellSpawner;
    private int myOccupiedWaypointIndex;
    [SerializeField] private Pickupable_Item pickupableItem;

    [SerializeField] private bool amIaSpell = false;

    private ObjectPool<PoolSpawnableObject> _pool;

    void Start()
    {
        spellSpawner = SpellSpawner.instance;
    }

    public void SetOccupiedWaypoint(int myWaypointIndex, PickupableBonusesSpawner spawnerScript)
    {
        if (pickupableBonusesSpawner == null)
        {
            pickupableBonusesSpawner = spawnerScript;
        }
        myOccupiedWaypointIndex = myWaypointIndex;
        pickupableBonusesSpawner.isSpawnPointTaken[myOccupiedWaypointIndex] = true;
    }

    public void FreeUpSlot() //What happens after picking the spell up
    {
        if (!amIaSpell)
        {
            pickupableBonusesSpawner.isSpawnPointTaken[myOccupiedWaypointIndex] = false;
            pickupableBonusesSpawner.howManyToSpawn++;
            pickupableBonusesSpawner.howManyToSpawn = Mathf.Clamp(pickupableBonusesSpawner.howManyToSpawn, 0, pickupableBonusesSpawner.startingHowManyToSpawn);
            pickupableBonusesSpawner.allPotionsPool.Release(this);
            pickupableBonusesSpawner.avaliableSpawnPointsCount++;
            pickupableBonusesSpawner.avaliableSpawnPointsCount = Mathf.Clamp(pickupableBonusesSpawner.avaliableSpawnPointsCount, 0, pickupableBonusesSpawner.spawnPoints.Length);
            return;
        }
    }

    public void SetPool(ObjectPool<PoolSpawnableObject> pool)
    {
        _pool = pool;
    }

}
