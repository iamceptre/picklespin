using UnityEngine;
using UnityEngine.AI;

public class WaypointWander : State
{
    [SerializeField] private Transform[] waypoints;
    [HideInInspector]public NavMeshAgent agent;
    private int waypointIndex;
    [HideInInspector] public Vector3 target;

    public AiVision aiVision;
    public AttackPlayer attackPlayer;

    private bool canInc=true;

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
        waypointIndex = 0;
        agent = GetComponentInParent<NavMeshAgent>();
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