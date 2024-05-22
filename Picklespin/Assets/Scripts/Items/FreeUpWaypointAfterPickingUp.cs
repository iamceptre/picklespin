using UnityEngine;

public class FreeUpWaypointAfterPickingUp : MonoBehaviour
{
    private PickupableBonusesSpawner pickupableBonusesSpawner;
    private SpellSpawner spellSpawner;
    public int myOccupiedWaypoint;

    [SerializeField] private bool amIaSpell = false;

    void Start()
    {
        pickupableBonusesSpawner = PickupableBonusesSpawner.instance;
        spellSpawner = SpellSpawner.instance;
    }

    public void FreeUpSlot()
    {
        if (!amIaSpell)
        {
            pickupableBonusesSpawner.TakenSpawnPoints.Remove(myOccupiedWaypoint);
            if (pickupableBonusesSpawner.howManyToSpawn<pickupableBonusesSpawner.startingHowManyToSpawn)
            {
                pickupableBonusesSpawner.howManyToSpawn++;
            }
        }
        else
        {
            spellSpawner.isSpawnPointTaken[myOccupiedWaypoint] = false;
            spellSpawner.howManyToSpawn++;
        }
    }

}
