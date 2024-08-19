using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

public class LoosingPlayer : State
{

    [SerializeField] private AttackPlayer attackPlayer;

    [SerializeField] private WaypointsForSpawner waypointWander;

    private AiVision aiVision;
    [SerializeField] private StateManager stateManager;

    [HideInInspector] public float currentTimedown;
    [HideInInspector] public float loosingTimedown;

    [HideInInspector] public bool lostPlayer = true;


    private void Awake()
    {
        loosingTimedown = 4f; //this amount of seconds will the ai be searching for player
        

    }

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
            currentTimedown = loosingTimedown;
        }
    }
}
