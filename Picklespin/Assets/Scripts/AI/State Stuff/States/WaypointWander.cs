using UnityEngine;
using UnityEngine.AI;

public class WaypointWander : State
{
    [SerializeField] private Transform[] waypoints;
    private NavMeshAgent agent;
    private int waypointIndex;
    [HideInInspector] public Vector3 target;
    private AiVision aiVision;
    [SerializeField] private AttackPlayer attackPlayer;
    private bool canInc=true;
    [SerializeField] private float idleSpeed = 3.5f;
    private Vector3 startingPos;

    public override State RunCurrentState()
    {
        if (aiVision.seeingPlayer)
        {
            return attackPlayer;
        }
        else
        {
            if (waypoints.Length > 0) //if any waypoints, then do the logic, if not, stay freezed
            {
                RefreshWaypoint();
            }
           return this;
        }
    }

    private void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        aiVision = GetComponentInParent<AiVision>();
        waypointIndex = 0;

        if (waypoints.Length > 0)
        {
            UpdateDestination();
        }
    }

    private void Start()
    {
        startingPos = transform.position;
    }


    private void RefreshWaypoint()
    {

            if (Vector3.Distance(transform.position, target) < 1)
            {
                Invoke("UpdateDestination", 2);

                if (canInc)
                {
                    IncreaseWaypoint();
                    canInc = false;
                }

        }

        if (Vector3.Distance(transform.position, target) >= 1.5f) //makes sure ai increment it's waypoint only once, no matter refresh rate
        {
            canInc = true;
        }

    }

    public void UpdateDestination()
    {
        agent.speed = idleSpeed;
        if (waypoints.Length > 0)
        {
            target = waypoints[waypointIndex].position;
        }
        else
        {
            target = startingPos;

        }
            agent.SetDestination(target);
    }

    private void IncreaseWaypoint() {
        waypointIndex++;
        if(waypointIndex == waypoints.Length) {
            waypointIndex = 0;
       } 
    }

}