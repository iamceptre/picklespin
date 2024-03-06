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

    public override State RunCurrentState()
    {
        if (aiVision.seeingPlayer)
        {
            return attackPlayer;
        }
        else
        {
           RefreshWaypoint();
           return this;
        }
    }

    private void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        aiVision = GetComponentInParent<AiVision>();
        waypointIndex = 0;
        UpdateDestination();
    }


    private void RefreshWaypoint()
    {
        if (Vector3.Distance(transform.position, target) <= 1f)
        {
            Invoke("UpdateDestination", 2); 

            if (canInc) {
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
        target = waypoints[waypointIndex].position;
            agent.SetDestination(target);
    }


    void IncreaseWaypoint() {
        waypointIndex++;
        if(waypointIndex == waypoints.Length) {
            waypointIndex = 0;
       } 
    }


}