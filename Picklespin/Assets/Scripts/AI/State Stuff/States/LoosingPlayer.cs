using UnityEngine;

public class LoosingPlayer : State
{
    [SerializeField] AttackPlayer attackPlayer;
    [SerializeField] WaypointsForSpawner waypointWander;
    [SerializeField] StateManager stateManager;

    [HideInInspector] public float currentTimedown;
    [HideInInspector] public float loosingTimedown;
    [HideInInspector] public bool lostPlayer = true;

    void Awake()
    {
        loosingTimedown = 4f;
        currentTimedown = loosingTimedown;
        lostPlayer = true;
    }

    public override State RunCurrentState()
    {
        if (!lostPlayer)
        {
            if (currentTimedown > 0) currentTimedown -= stateManager.RefreshEveryVarSeconds;
            else
            {
                lostPlayer = true;
                currentTimedown = loosingTimedown;
            }
            return this;
        }
        return waypointWander;
    }

    public void ResetLoosingState()
    {
        currentTimedown = loosingTimedown;
        lostPlayer = true;
    }
}
