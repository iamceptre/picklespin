using Pathfinding;
using System.Collections;
using UnityEngine;

public class HelperSpirit : MonoBehaviour
{
    public static HelperSpirit instance { get; private set; }

    [SerializeField] private AIDestinationSetter aiDestinationSetter;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Light spiritLight;
    [SerializeField] private float angelDistanceThreshold = 12f;
    [SerializeField] private float playerDistanceThreshold = 2f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float checkInterval = 0.25f;

    private Transform targetAngel;
    private PublicPlayerTransform playerTransform;
    private bool isGoingToAngel = true;
    private float distanceThreshold;
    private float startingLightIntensity;
    private float startingTrailWidth;
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
    }

    private void Start()
    {
        playerTransform = PublicPlayerTransform.Instance;
        refreshInterval = new WaitForSeconds(checkInterval);
        startingLightIntensity = spiritLight.intensity;
        startingTrailWidth = trailRenderer.widthMultiplier;

        spiritLight.intensity = 0;
    }

    public void ShowSpirit(Transform target)
    {
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

    private IEnumerator FadeInAndInit(Transform target)
    {
        spiritLight.intensity = 0f;
        trailRenderer.widthMultiplier = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            spiritLight.intensity = Mathf.Lerp(0f, startingLightIntensity, t);
            trailRenderer.widthMultiplier = Mathf.Lerp(0f, startingTrailWidth, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spiritLight.intensity = startingLightIntensity;
        trailRenderer.widthMultiplier = startingTrailWidth;

        isGoingToAngel = true;
        distanceThreshold = angelDistanceThreshold;
        transform.position = playerTransform.PlayerTransform.position;
        trailRenderer.Clear();
        targetAngel = target;
        aiDestinationSetter.target = targetAngel;
        if (checkerCoroutine != null) StopCoroutine(checkerCoroutine);
        checkerCoroutine = StartCoroutine(DistanceChecker());
    }

    private IEnumerator HideSpiritRoutine()
    {
        float startLightIntensity = spiritLight.intensity;
        float startTrailWidth = trailRenderer.widthMultiplier;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            spiritLight.intensity = Mathf.Lerp(startLightIntensity, 0f, t);
            trailRenderer.widthMultiplier = Mathf.Lerp(startTrailWidth, 0f, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spiritLight.intensity = 0f;
        trailRenderer.widthMultiplier = 0f;
        fadeCoroutine = null;
        transitionCoroutine = null;
        checkerCoroutine = null;
    }

    private IEnumerator DistanceChecker()
    {
        while (true)
        {
            if (aiDestinationSetter.target != null)
            {
                float distance = Vector3.Distance(transform.position, aiDestinationSetter.target.position);
                if (distance <= distanceThreshold && transitionCoroutine == null)
                {
                    transitionCoroutine = StartCoroutine(HandleWaypointTransition());
                }
            }
            yield return refreshInterval;
        }
    }

    private IEnumerator HandleWaypointTransition()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutEffects());
        yield return new WaitForSeconds(fadeDuration);

        if (isGoingToAngel)
        {
            isGoingToAngel = false;
            transform.position = playerTransform.PlayerTransform.position;
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
        fadeCoroutine = StartCoroutine(FadeInEffects());
        yield return refreshInterval;
        transitionCoroutine = null;
    }

    private IEnumerator FadeOutEffects()
    {
        float startLightIntensity = spiritLight.intensity;
        float startTrailWidth = trailRenderer.widthMultiplier;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            spiritLight.intensity = Mathf.Lerp(startLightIntensity, 0f, t);
            trailRenderer.widthMultiplier = Mathf.Lerp(startTrailWidth, 0f, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spiritLight.intensity = 0f;
        trailRenderer.widthMultiplier = 0f;
    }

    private IEnumerator FadeInEffects()
    {
        spiritLight.intensity = 0f;
        trailRenderer.widthMultiplier = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            spiritLight.intensity = Mathf.Lerp(0f, startingLightIntensity, t);
            trailRenderer.widthMultiplier = Mathf.Lerp(0f, startingTrailWidth, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spiritLight.intensity = startingLightIntensity;
        trailRenderer.widthMultiplier = startingTrailWidth;
    }
}
