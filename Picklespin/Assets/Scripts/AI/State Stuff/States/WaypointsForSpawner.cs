using Pathfinding;
using UnityEngine;

public class WaypointsForSpawner : State
{
    [Header("Waypoints")]
    public Transform[] cachedPoint;
    [SerializeField] Transform[] waypoints;
    [SerializeField] int waypointIndex = 0;

    [Header("References")]
    [SerializeField] AIPath aiPath;
    [SerializeField] AIDestinationSetter destinationSetter;
    [SerializeField] AiVision aiVision;
    [SerializeField] AttackPlayer attackPlayer;

    [Header("Movement")]
    readonly float idleSpeed = 5f;
    readonly float rotationSpeed = 90f;

    bool canIncrement = true;
    readonly float waitOnWaypointTime = 2f;

    public Transform CurrentTarget;

    void Awake()
    {
        waypointIndex = 0;
    }

    void Start()
    {
        RandomizeWaypoints();
        UpdateDestination();
    }

    public override State RunCurrentState()
    {
        if (aiVision.seeingPlayer)
            return attackPlayer;

        aiPath.maxSpeed = idleSpeed;

        if (waypoints != null && waypoints.Length > 0)
        {
            if (CurrentTarget == null || destinationSetter.target != CurrentTarget)
                destinationSetter.target = waypoints[waypointIndex];
        }
        else
        {
            destinationSetter.target = null;
        }

        RefreshWaypoint();
        return this;
    }

    void RefreshWaypoint()
    {
        if (CurrentTarget == null) return;
        float distance = Vector3.Distance(transform.position, CurrentTarget.position);

        if (distance < 6f && canIncrement)
        {
            Invoke(nameof(UpdateDestination), waitOnWaypointTime);
            IncrementWaypoint();
        }
        if (distance > 6f && !canIncrement) canIncrement = true;
    }

    void UpdateDestination()
    {
        aiPath.maxSpeed = idleSpeed;
        aiPath.rotationSpeed = rotationSpeed;
        if (waypoints.Length > 0)
        {
            CurrentTarget = waypoints[waypointIndex];
            destinationSetter.target = CurrentTarget;
        }
        else
        {

            CurrentTarget = null;
            destinationSetter.target = null;
        }
    }

    void IncrementWaypoint()
    {
        canIncrement = false;
        waypointIndex++;
        if (waypointIndex >= waypoints.Length) waypointIndex = 0;
    }

    void RandomizeWaypoints()
    {
        if (cachedPoint == null || cachedPoint.Length == 0) return;
        waypoints = new Transform[cachedPoint.Length];
        int previousNumber = -1;
        for (int i = 0; i < cachedPoint.Length; i++)
        {
            int rand;
            do { rand = Random.Range(0, cachedPoint.Length); }
            while (rand == previousNumber);

            previousNumber = rand;
            waypoints[i] = cachedPoint[rand];
        }
    }

    public void ResetWaypointState()
    {
        CancelInvoke();
        waypointIndex = 0;
        canIncrement = true;
        CurrentTarget = null;
        RandomizeWaypoints();
        UpdateDestination();
    }
}
