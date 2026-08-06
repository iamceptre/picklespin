using Pathfinding;
using System.Collections;
using UnityEngine;

public class HelperSpirit : MonoBehaviour
{
    public static HelperSpirit instance { get; private set; }

    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Light spiritLight;
    [SerializeField] private SpriteRenderer StaticSprite;
    [SerializeField] private float angelDistanceThreshold = 12f;
    [SerializeField] private float playerDistanceThreshold = 2f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float checkInterval = 0.25f;

    [Header("Guiding")]
    [SerializeField, Tooltip("the spirit only advances while the player is within this range")]
    private float followRadius = 11f;
    [SerializeField, Tooltip("if the player gets this much closer to the angel than the spirit, it skips ahead")]
    private float overtakeMargin = 4f;
    [SerializeField, Tooltip("how quickly the spirit's speed eases toward its target (higher = snappier)")]
    private float speedSmoothing = 3f;

    private float baseSpeed;

    private Transform targetAngel;
    private PublicPlayerTransform playerTransform;
    private bool isGoingToAngel = true;
    private float distanceThreshold;
    private float startingLightIntensity;
    private float startingTrailWidth;
    private float startingSpriteAlpha;
    private WaitForSeconds refreshInterval;
    private Coroutine fadeCoroutine;
    private Coroutine transitionCoroutine;
    private Coroutine checkerCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (!aiPath) TryGetComponent(out aiPath);

        refreshInterval = new WaitForSeconds(checkInterval);
        startingLightIntensity = spiritLight.intensity;
        startingTrailWidth = trailRenderer.widthMultiplier;
        if (StaticSprite) startingSpriteAlpha = StaticSprite.color.a;
        if (aiPath) baseSpeed = aiPath.maxSpeed;

        SetEffectsStrength(0f);
    }

    private void Start()
    {
        EnsurePlayer();
    }

    private void EnsurePlayer()
    {
        if (playerTransform == null) playerTransform = PublicPlayerTransform.Instance;
    }

    private void Update()
    {
        if (!aiPath || playerTransform == null) return;

        // eases to a halt as the player falls behind; the way back is always full speed
        float targetFactor = 1f;
        if (isGoingToAngel)
        {
            float playerDistance = Vector3.Distance(transform.position, playerTransform.PlayerTransform.position);
            targetFactor = 1f - Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(followRadius * 0.6f, followRadius, playerDistance));
        }

        float ease = 1f - Mathf.Exp(-speedSmoothing * Time.deltaTime);
        aiPath.maxSpeed = Mathf.Lerp(aiPath.maxSpeed, baseSpeed * targetFactor, ease);
    }

    public void ShowSpirit(Transform target)
    {
        EnsurePlayer();
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeInAndInit(target));
    }

    public void HideSpirit()
    {
        if (checkerCoroutine != null) StopCoroutine(checkerCoroutine);
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(HideSpiritRoutine());
    }

    private void SetEffectsStrength(float strength)
    {
        spiritLight.intensity = startingLightIntensity * strength;
        trailRenderer.widthMultiplier = startingTrailWidth * strength;
        if (StaticSprite)
        {
            StaticSprite.enabled = strength > 0f;
            Color color = StaticSprite.color;
            color.a = startingSpriteAlpha * strength;
            StaticSprite.color = color;
        }
    }

    private float CurrentStrength =>
        startingLightIntensity > 0f ? spiritLight.intensity / startingLightIntensity : 0f;

    private IEnumerator FadeEffectsTo(float target)
    {
        float start = CurrentStrength;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            SetEffectsStrength(Mathf.Lerp(start, target, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SetEffectsStrength(target);
    }

    private void TeleportTo(Vector3 position)
    {
        if (aiPath) aiPath.Teleport(position);
        else transform.position = position;
    }

    private IEnumerator FadeInAndInit(Transform target)
    {
        SetEffectsStrength(0f);
        yield return FadeEffectsTo(1f);

        isGoingToAngel = true;
        distanceThreshold = angelDistanceThreshold;
        TeleportTo(playerTransform.PlayerTransform.position);
        trailRenderer.Clear();
        targetAngel = target;
        aiDestinationSetter.target = targetAngel;
        if (aiPath) aiPath.canMove = true;
        if (checkerCoroutine != null) StopCoroutine(checkerCoroutine);
        checkerCoroutine = StartCoroutine(DistanceChecker());
    }

    private IEnumerator HideSpiritRoutine()
    {
        yield return FadeEffectsTo(0f);
        fadeCoroutine = null;
        transitionCoroutine = null;
        checkerCoroutine = null;
    }

    private IEnumerator DistanceChecker()
    {
        while (true)
        {
            Transform target = aiDestinationSetter.target;
            if (target != null && transitionCoroutine == null)
            {
                Vector3 playerPosition = playerTransform.PlayerTransform.position;
                float spiritToTarget = Vector3.Distance(transform.position, target.position);

                if (spiritToTarget <= distanceThreshold)
                {
                    transitionCoroutine = StartCoroutine(HandleWaypointTransition());
                }
                else if (isGoingToAngel)
                {
                    // the player overtook the guide: skip ahead so it never trails
                    float playerToTarget = Vector3.Distance(playerPosition, target.position);
                    if (playerToTarget + overtakeMargin < spiritToTarget)
                    {
                        transitionCoroutine = StartCoroutine(SkipAheadToPlayer());
                    }
                }
            }
            yield return refreshInterval;
        }
    }

    private IEnumerator SkipAheadToPlayer()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeEffectsTo(0f));
        yield return new WaitForSeconds(fadeDuration);

        TeleportTo(playerTransform.PlayerTransform.position);
        if (aiPath) aiPath.canMove = true;
        trailRenderer.Clear();
        fadeCoroutine = StartCoroutine(FadeEffectsTo(1f));
        yield return refreshInterval;
        transitionCoroutine = null;
    }

    private IEnumerator HandleWaypointTransition()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeEffectsTo(0f));
        yield return new WaitForSeconds(fadeDuration);

        if (isGoingToAngel)
        {
            isGoingToAngel = false;
            TeleportTo(playerTransform.PlayerTransform.position);
            aiDestinationSetter.target = playerTransform.PlayerTransform;
            distanceThreshold = playerDistanceThreshold;
        }
        else
        {
            isGoingToAngel = true;
            aiDestinationSetter.target = targetAngel;
            distanceThreshold = angelDistanceThreshold;
        }

        trailRenderer.Clear();
        fadeCoroutine = StartCoroutine(FadeEffectsTo(1f));
        yield return refreshInterval;
        transitionCoroutine = null;
    }
}
