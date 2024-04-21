using UnityEngine;
using UnityEngine.AI;

public class LoosingPlayer : State
{

    [SerializeField] private AttackPlayer attackPlayer;

    [SerializeField] private WaypointWander waypointWander;
    [SerializeField] private WaypointsForSpawner waypointWanderForSpawnedOnes;

    private AiVision aiVision;
    private StateManager stateManager;
    private NavMeshAgent agent;

    [HideInInspector] public float currentTimedown;
    [HideInInspector] public float loosingTimedown;

    [HideInInspector] public bool lostPlayer = true;

    private float loosingPlayerSpeed = 6;

    private void Awake()
    {
        loosingTimedown = 4f; //this amount of seconds will the ai be searching for player
        agent = GetComponentInParent<NavMeshAgent>();
        aiVision = GetComponentInParent<AiVision>();
        stateManager = GetComponentInParent<StateManager>();

        if (waypointWander == null) //it means that the ai was spawned
        {
            var parent = transform.parent.gameObject;
            waypointWanderForSpawnedOnes = parent.GetComponentInChildren<WaypointsForSpawner>();
        }

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
            spawnedNullFix();
            return waypointWander;
        }
    }

    void spawnedNullFix()
    {
        if (waypointWander == null)
        {
            waypointWanderForSpawnedOnes.UpdateDestination();
        }
        else
        {
            waypointWander.UpdateDestination();
        }
    }

    void LoosingSight()
    {
        if (currentTimedown > 0)
        {
            currentTimedown -= stateManager.RefreshEveryVarSeconds;
            agent.speed = loosingPlayerSpeed;
        }
        else
        {
            lostPlayer = true;
            currentTimedown = loosingTimedown;
        }
    }
}
