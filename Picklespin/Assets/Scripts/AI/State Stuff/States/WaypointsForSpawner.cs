using Pathfinding;
using UnityEngine;

public class WaypointsForSpawner : State
{
    public Transform[] cachedPoint;
    [SerializeField] private Transform[] waypoints;

    [SerializeField] private AIPath aiPath;
    [SerializeField] private AIDestinationSetter destinationSetter;
    [SerializeField] private int waypointIndex;
    [HideInInspector] public Transform CurrentTarget;
    [SerializeField] private AiVision aiVision;
    [SerializeField] private AttackPlayer attackPlayer;
    private bool canIncrement = true;
    [SerializeField] private float idleSpeed = 3.5f;
    private Vector3 startingPos;

    private Vector3 previousPosition;
    private float velocity;
    private Vector3 velocityVector;

    private float waitOnWaypointTime = 2;


    private int rrrandom;
    private int previousNumber = -1;
    
    public override State RunCurrentState() //This shit runs periodically 
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
    }



    private void Start()
    {
        startingPos = transform.position;
        RandomizeWaypoints();
    }

    private void CalculateVelocity()
    {
        velocityVector = transform.position - previousPosition;
        velocity = velocityVector.magnitude;
        previousPosition = transform.position;
    }


    private void RefreshWaypoint()
    {

        if (CurrentTarget != null)
        {

            if (Vector3.Distance(transform.position, CurrentTarget.position) < 1 && canIncrement)
            {
                Invoke("UpdateDestination", waitOnWaypointTime);
                IncreaseWaypoint();
            }

            if (Vector3.Distance(transform.position, CurrentTarget.position) > 2 && !canIncrement)
            {
                canIncrement = true;
            }
        }

    }


    public void UpdateDestination()
    {
        aiPath.maxSpeed = idleSpeed;

        if (waypoints.Length > 0)
        {
            CurrentTarget = waypoints[waypointIndex];
        }
        else
        {
            CurrentTarget = null;

        }
        destinationSetter.target = CurrentTarget;
    }


    private void IncreaseWaypoint()
    {
        canIncrement = false;
        waypointIndex++;
        if (waypointIndex == waypoints.Length)
        {
            waypointIndex = 0;
        }
    }

    private void RandomizeWaypoints()
    {
        waypoints = new Transform[cachedPoint.Length];

        for (int i = 0; i < cachedPoint.Length; i++)
        {
            RoundRobin();
            waypoints[i] = cachedPoint[rrrandom];
        }

        RefreshWaypoint();
        UpdateDestination();
    }

    private void RoundRobin()
    {
        do
        {
            rrrandom = Random.Range(0, cachedPoint.Length);
        } while (rrrandom == previousNumber);

        previousNumber = rrrandom;
    }


}