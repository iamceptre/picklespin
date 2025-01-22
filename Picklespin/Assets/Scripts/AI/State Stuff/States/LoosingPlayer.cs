using UnityEngine;
using Pathfinding; 

public class LoosingPlayer : State
{
    [Header("References")]
    [SerializeField] AttackPlayer attackPlayer;
    [SerializeField] WaypointsForSpawner waypointWander;
    [SerializeField] StateManager stateManager;

    [SerializeField] AiVision aiVision;
    [SerializeField] AIDestinationSetter destinationSetter;
    [SerializeField] AIPath aiPath;

    [HideInInspector] public float currentTimedown;
    [HideInInspector] public float loosingTimedown = 4f;



    private void Awake()
    {
        loosingTimedown = 4f;
        currentTimedown = loosingTimedown;
    }

    public override State RunCurrentState()
    {
        if (ReallyReacquiredPlayer())
        {
            currentTimedown = loosingTimedown;
            return attackPlayer;
        }

        currentTimedown -= stateManager.RefreshEveryVarSeconds;

        if (currentTimedown <= 0)
        {
            currentTimedown = loosingTimedown;

            aiVision.seeingPlayer = false;

            return waypointWander;
        }


        aiVision.seeingPlayer = true;


        if (destinationSetter.target != aiVision.playerRef)
        {
            destinationSetter.target = aiVision.playerRef;
        }

        aiPath.maxSpeed = 6f;
        aiPath.rotationSpeed = 150f;

        return this;
    }

    public void StartLoosingState()
    {
        currentTimedown = loosingTimedown;
    }

    public void ResetLoosingState()
    {
        currentTimedown = loosingTimedown;
    }

    private bool ReallyReacquiredPlayer()
    {
        if (aiVision.playerJustHitMe) return true;

        Vector3 dir = (aiVision.playerRef.position - transform.position).normalized;
        float fovAngle = Vector3.Angle(transform.forward, dir);
        if (fovAngle < aiVision.angle * 0.5f)
        {
            float dist = Vector3.Distance(transform.position, aiVision.playerRef.position);
            bool blocked = Physics.Raycast(transform.position, dir, dist, aiVision.obstructionMask);
            if (!blocked) return true;
        }


        return false;
    }
}
