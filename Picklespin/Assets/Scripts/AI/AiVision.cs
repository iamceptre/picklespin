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
    private float hitMeCooldown;

    public static List<AiVision> AllAIs { get; } = new();

    void OnEnable() => AllAIs.Add(this);
    void OnDisable() => AllAIs.Remove(this);

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
        if (playerJustHitMe)
        {
            seeingPlayer = true;
            return;
        }

        FieldOfViewCheck();
        HearingCheck();
    }

    private void FieldOfViewCheck()
    {
        Vector3 dir = (playerRef.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dir) < angle * 0.5f)
        {
            float dist = Vector3.Distance(transform.position, playerRef.position);
            seeingPlayer = !Physics.Raycast(transform.position, dir, dist, obstructionMask);
        }
        else
        {
            seeingPlayer = false;
        }
    }

    private void HearingCheck()
    {
        if (playerMovement.movementStateForFMOD == 0) return;
        float distance = Vector3.Distance(transform.position, playerRef.position);

        if (landingHearingActive)
        {
            seeingPlayer = distance <= landHearingRange;
            return;
        }

        seeingPlayer = playerMovement.movementStateForFMOD == 2
            ? distance <= runHearingRange
            : distance <= walkHearingRange;
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

    public static bool IsAnyEnemySeeingPlayer()
    {
        foreach (AiVision ai in AllAIs)
        {
            if (ai.seeingPlayer) return true;
        }
        return false;
    }

    public static void UpdateDetectionIcon(GameObject detectionIcon)
    {
        if (detectionIcon == null) return;
        detectionIcon.SetActive(IsAnyEnemySeeingPlayer());
    }
}
