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

    [Header("Search")]
    [SerializeField, Tooltip("how long the enemy keeps hunting the spot it last saw the player at")]
    float loosingTimedown = 4f;
    [SerializeField] float searchSpeed = 6f;
    [SerializeField] float searchRotationSpeed = 150f;

    float currentTimedown;

    public bool HasGrudge => attackPlayer && attackPlayer.HasGrudge;

    void Awake() => currentTimedown = loosingTimedown;

    public override State RunCurrentState()
    {

        if (attackPlayer.HasGrudge) return attackPlayer;

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

        aiPath.maxSpeed = searchSpeed * WishUpgrades.EnemySpeedMultiplier * ConvertedAlly.SpeedMultiplierAt(transform.position);
        aiPath.rotationSpeed = searchRotationSpeed;

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

    private bool ReallyReacquiredPlayer() => aiVision.playerJustHitMe || aiVision.CanSeePlayer();
}
