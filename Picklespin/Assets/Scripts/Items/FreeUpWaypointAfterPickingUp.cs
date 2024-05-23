using UnityEngine;

public class FreeUpWaypointAfterPickingUp : MonoBehaviour
{
    private PickupableBonusesSpawner pickupableBonusesSpawner;
    private SpellSpawner spellSpawner;
    private Transform myOccupiedWaypoint;

    [SerializeField] private bool amIaSpell = false;

    void Start()
    {
        //pickupableBonusesSpawner = PickupableBonusesSpawner.instance;
        spellSpawner = SpellSpawner.instance;
    }

    public void SetOccupiedWaypoint(Transform myWaypoint, PickupableBonusesSpawner spawnerScript)
    {
        pickupableBonusesSpawner = spawnerScript;
        myOccupiedWaypoint = myWaypoint;
        pickupableBonusesSpawner.AvaliableSpawnPoints.Remove(myWaypoint);
    }

    public void FreeUpSlot() //What happens after picking the spell up
    {
        if (!amIaSpell)
        {
            //pickupableBonusesSpawner.TakenSpawnPoints.Remove(myOccupiedWaypoint);
            pickupableBonusesSpawner.AvaliableSpawnPoints.Add(myOccupiedWaypoint);
            if (pickupableBonusesSpawner.howManyToSpawn<pickupableBonusesSpawner.startingHowManyToSpawn)
            {
                pickupableBonusesSpawner.howManyToSpawn++;
            }
        }
        else
        {
            //ADD SPELL AVALIABLE WAYPOINT TO LIST
            //spellSpawner.isSpawnPointTaken[myOccupiedWaypoint] = false;
            spellSpawner.howManyToSpawn++;
        }
    }

}
