using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WaypointsForSpawner : State
{
    public Transform[] cachedPoint;
    [SerializeField] private Transform[] waypoints;

    private NavMeshAgent agent;
    [SerializeField] private int waypointIndex;
    [HideInInspector] public Vector3 target;
    private AiVision aiVision;
    [SerializeField] private AttackPlayer attackPlayer;
    private bool canIncrement = true;
    [SerializeField] private float idleSpeed = 3.5f;
    private Vector3 startingPos;

    private Vector3 previousPosition;
    private float velocity;
    private Vector3 velocityVector;

    private float waitOnWaypointTime = 2;

    //bool imStuck;

    private int rrrandom;
    private int previousNumber = -1;

    private WaitForSeconds stuckCheckTime = new WaitForSeconds(1);

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
        agent = GetComponentInParent<NavMeshAgent>();
        aiVision = GetComponentInParent<AiVision>();
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

        //CalculateVelocity();
        //StuckCheck();

        if (Vector3.Distance(transform.position, target) < 1 && canIncrement)
        {
            Invoke("UpdateDestination", waitOnWaypointTime);
            IncreaseWaypoint();
        }

        if (Vector3.Distance(transform.position, target) > 2 && !canIncrement)
        {
            canIncrement = true;
        }

    }


    private void StuckCheck()
    {
        if (velocity < 0.2)
        {
            StartCoroutine(StuckCheckTimer());
        }
        else
        {
            StopAllCoroutines();
        }
    }


    private IEnumerator StuckCheckTimer()
    {
        yield return stuckCheckTime;
        //imStuck = true;
        canIncrement = true;
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


    private void IncreaseWaypoint()
    {
        canIncrement = false;
        waypointIndex++;
        //imStuck = false;
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