using UnityEngine;

public class CanvasGroupOpacityAnimator : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private float targetAlpha = 1;
    private float animationDuration = 0.25f;

    private float startAlpha;
    private float startTime;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        startAlpha = canvasGroup.alpha;
        canvasGroup.alpha = 0;
        startTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / animationDuration);
        canvasGroup.alpha = newAlpha;

        if (newAlpha >= 1)
        {
            enabled = false;
        }
    }
}