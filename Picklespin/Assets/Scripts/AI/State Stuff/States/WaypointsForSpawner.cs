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
    private bool canInc=true;
    [SerializeField] private float idleSpeed = 3.5f;
    private Vector3 startingPos;

    private Vector3 previousPosition;
    private float velocity;
    private Vector3 velocityVector;

    private float waitOnWaypointTime = 2;

    bool imStuck;

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
        agent = GetComponentInParent<NavMeshAgent>();
        aiVision = GetComponentInParent<AiVision>();
        waypointIndex = 0;
    }



    private void Start()
    {
        startingPos = transform.position;
        RandomizeWaypoints();
    }


    private void RefreshWaypoint()
    {

        velocityVector = transform.position - previousPosition;
        velocity = velocityVector.magnitude;
        previousPosition = transform.position;

        if (velocity < 0.2)
        {
            StartCoroutine(StuckCheck());
        }
        else
        {
            StopAllCoroutines();
        }

        if (Vector3.Distance(transform.position, target) < 1 || imStuck)
            {
                Invoke("UpdateDestination", waitOnWaypointTime);

                if (canInc)
                {
                    IncreaseWaypoint();
                    canInc = false;
                    Invoke("CanInc", waitOnWaypointTime * 0.5f);
                }
        }


        if (Vector3.Distance(transform.position, target) > 8)
        {
            canInc = true;
        }

    }

    private void CanInc()
    {
        canInc = true;
    }
   

    private IEnumerator StuckCheck()
    {
        yield return new WaitForSeconds(1);
        imStuck = true;
        canInc = true;
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
        imStuck = false;
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
        //ROUND ROBIN TIME BABYYYY!
        do
        {
            rrrandom = Random.Range(0, cachedPoint.Length);
        } while (rrrandom == previousNumber); 

        previousNumber = rrrandom; 
    }


}