using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiVision : MonoBehaviour
{
    [Header("Vision")]
    public float radius = 45f;
    [Range(0, 360)] public float angle;

    [HideInInspector] public Transform playerRef;
    private PlayerMovement playerMovement;

    [Header("Layer Masks")]
    public LayerMask obstructionMask;

    [Header("Hearing")]
    public static readonly float walkHearingRange = 15f;
    public static readonly float runHearingRange = 30f;
    public static readonly float landHearingRange = 45f;

    public bool landingHearingActive;
    private static readonly WaitForSeconds landingTime = new(1);

    public bool seeingPlayer;
    public bool playerJustHitMe;
    private float hitMeUntilTime;

    public static List<AiVision> AllAIs { get; } = new();

    // angle is public and tweakable, so recompute only when it actually changes
    private float cachedAngle = -1f;
    private float cachedCosHalfAngle;
    private float CosHalfAngle
    {
        get
        {
            if (!Mathf.Approximately(cachedAngle, angle))
            {
                cachedAngle = angle;
                cachedCosHalfAngle = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            }
            return cachedCosHalfAngle;
        }
    }

    void OnEnable() => AllAIs.Add(this);
    void OnDisable() => AllAIs.Remove(this);

    void Start() => ResolvePlayerRefs();

    void ResolvePlayerRefs()
    {
        if (!playerRef && CachedCameraMain.instance) playerRef = CachedCameraMain.instance.cachedTransform;
        if (!playerMovement) playerMovement = PlayerMovement.Instance;
    }

    public void PerceptionCheck()
    {
        if (AngelArea.PlayerInside)
        {
            seeingPlayer = false;
            playerJustHitMe = false;
            hitMeUntilTime = 0;
            return;
        }

        if (!playerRef)
        {
            ResolvePlayerRefs(); // the FSM can tick before Start on a scene-placed enemy
            if (!playerRef) return;
        }

        if (playerJustHitMe)
        {
            if (Time.time < hitMeUntilTime)
            {
                seeingPlayer = true;
                return;
            }
            playerJustHitMe = false;
        }

        Vector3 toPlayer = playerRef.position - transform.position;
        float distance = toPlayer.magnitude;

        // sight OR hearing: either alone keeps the player acquired
        seeingPlayer = CanSeePlayer(toPlayer, distance) || CanHearPlayer(distance);
    }

    public bool CanSeePlayer()
    {
        if (AngelArea.PlayerInside) return false;
        if (!playerRef) return false;
        Vector3 toPlayer = playerRef.position - transform.position;
        return CanSeePlayer(toPlayer, toPlayer.magnitude);
    }

    private bool CanSeePlayer(Vector3 toPlayer, float distance)
    {
        if (distance > radius || distance <= Mathf.Epsilon) return false;

        Vector3 dir = toPlayer / distance;
        // dot against the cached cosine: Vector3.Angle without the acos, per tick
        if (Vector3.Dot(transform.forward, dir) < CosHalfAngle) return false;

        return !Physics.Raycast(transform.position, dir, distance, obstructionMask);
    }

    private bool CanHearPlayer(float distance)
    {
        if (!playerMovement || playerMovement.movementStateForFMOD == 0) return false;

        if (landingHearingActive) return distance <= landHearingRange;

        return playerMovement.movementStateForFMOD == 2
            ? distance <= runHearingRange
            : distance <= walkHearingRange;
    }

    public void ResetVisionState()
    {
        seeingPlayer = false;
        playerJustHitMe = false;
        hitMeUntilTime = 0;
        landingHearingActive = false;
    }

    public void HitShowsMePlayer()
    {
        hitMeUntilTime = Time.time + 6f;
        playerJustHitMe = true;
    }

    public IEnumerator EnableLandingHearing()
    {
        landingHearingActive = true;
        yield return landingTime;
        landingHearingActive = false;
    }
}
