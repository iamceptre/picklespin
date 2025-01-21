using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiVision : MonoBehaviour
{
    [Header("Vision")]
    public float radius = 45;
    [Range(0, 360)] public float angle;

    [HideInInspector] public Transform playerRef;
    private PlayerMovement playerMovement;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstructionMask;

    [Header("Hearing")]
    public static readonly float walkHearingRange = 9f;
    public static readonly float runHearingRange = 25f;
    public static readonly float landHearingRange = 40f;

    public bool landingHearingActive;
    static readonly WaitForSeconds landingTime = new(1);

    public bool seeingPlayer;
    public bool playerJustHitMe;
    public float hitMeCooldown;

    static readonly Collider[] nonAllocResults = new Collider[10];
    public static List<AiVision> AllAIs { get; } = new List<AiVision>();

    void OnEnable()
    {
        AllAIs.Add(this);
    }

    void OnDisable()
    {
        AllAIs.Remove(this);
    }

    void Start()
    {
        playerRef = CachedCameraMain.instance.cachedTransform;
        playerMovement = PlayerMovement.Instance;
    }

    void Update()
    {
        if (hitMeCooldown > 0)
        {
            hitMeCooldown -= Time.deltaTime;
            if (hitMeCooldown <= 0)
            {
                hitMeCooldown = 0;
                playerJustHitMe = false;
            }
        }
    }

    public void PerceptionCheck()
    {
        FieldOfViewCheck();
        HearingCheck();
    }

    void FieldOfViewCheck()
    {
        if (playerJustHitMe)
        {
            seeingPlayer = true;
            return;
        }

        System.Array.Clear(nonAllocResults, 0, nonAllocResults.Length);
        int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, nonAllocResults, targetMask);

        if (hits > 0)
        {
            Vector3 dir = (playerRef.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dir) < angle * 0.5f)
            {
                float dist = Vector3.Distance(transform.position, playerRef.position);
                seeingPlayer = !Physics.Raycast(transform.position, dir, dist, obstructionMask);
            }
            else seeingPlayer = false;
        }
        else seeingPlayer = false;
    }

    void HearingCheck()
    {
        if (playerMovement == null || playerMovement.movementStateForFMOD == 0) return;
        float distance = Vector3.Distance(transform.position, playerRef.position);

        if (landingHearingActive)
        {
            if (distance <= landHearingRange) seeingPlayer = true;
            return;
        }

        if (playerMovement.movementStateForFMOD == 2)
        {
            if (distance <= runHearingRange) seeingPlayer = true;
        }
        else
        {
            if (distance <= walkHearingRange) seeingPlayer = true;
        }
    }

    public void ResetVisionState()
    {
        seeingPlayer = false;
        playerJustHitMe = false;
        hitMeCooldown = 0;
        landingHearingActive = false;
    }

    public void HitShowsMePlayer()
    {
        hitMeCooldown = 6f;
        playerJustHitMe = true;
    }

    public IEnumerator EnableLandingHearing()
    {
        landingHearingActive = true;
        yield return landingTime;
        landingHearingActive = false;
    }
}
