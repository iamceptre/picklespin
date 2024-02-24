using UnityEngine;

public class LoosingPlayer : State
{

    public AttackPlayer attackPlayer;
    public WaypointWander waypointWander;
    public AiVision aiVision;
    public StateManager stateManager;


    [HideInInspector] public float currentTimedown;
    [HideInInspector] public float loosingTimedown = 3f;

    public bool lostPlayer = true;

    public override State RunCurrentState()
    {
        if (!lostPlayer)
        {
            if (aiVision.seeingPlayer)
            {
                return attackPlayer;
            }
            else
            {
                LoosingSight();
                return this;
            }
        }
        else
        {
            waypointWander.UpdateDestination(); //later make AI return to the closest waypoint intead of the last one, make a FindClosestWaypoint function and run it here
            return waypointWander;
        }
    }

    void LoosingSight()
    {
        if (currentTimedown > 0)
        {
            currentTimedown -= stateManager.RefreshEveryVarSeconds;
        }
        else
        {
            lostPlayer = true;
        }
    }
}
