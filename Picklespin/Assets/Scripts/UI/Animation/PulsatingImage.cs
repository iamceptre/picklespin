using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PulsatingImage : MonoBehaviour
{
    [Header("Image Reference")]
    [SerializeField] private Image targetImage;

    [Header("Pulsating Settings")]
    [SerializeField] private float minAlpha = 0.32f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float pulsateSpeed = 2f;

    private bool isPulsating = false;

    private float alphaRange;
    private float pulsateFrequency;
    private WaitForEndOfFrame waitForEndOfFrame;
    private IEnumerator pulsateEnumerator;

    private void Awake()
    {
        alphaRange = maxAlpha - minAlpha;
        pulsateFrequency = pulsateSpeed * Mathf.PI * 2;
        waitForEndOfFrame = new WaitForEndOfFrame();
        pulsateEnumerator = Pulsate();
    }

    // the threshold is passed in, never stored: a second copy in the Inspector would
    // drift from the system that owns it. Idempotent, so per-frame calls are fine.
    public void RefreshLowState(float fraction, float lowThreshold)
    {
        if (fraction < lowThreshold) StartPulsating();
        else StopPulsating();
    }

    public void StartPulsating()
    {
        if (isPulsating) return;
        // StartCoroutine throws on a deactivated object, and PlayerClassHud switches
        // whole bars off by class
        if (!isActiveAndEnabled) return;
        isPulsating = true;
        StopCoroutine(pulsateEnumerator);
        StartCoroutine(pulsateEnumerator);
    }

    public void StopPulsating()
    {
        if (!isPulsating) return;
        isPulsating = false;
        StopCoroutine(pulsateEnumerator);
        SetImageAlpha(maxAlpha);
    }

    private IEnumerator Pulsate()
    {
        float time = 0f;

        while (true)
        {
            if (!isPulsating || targetImage == null)
            {
                yield return null;
                continue;
            }

            float sinValue = Mathf.Sin(time) * 0.5f + 0.5f;
            float alpha = minAlpha + sinValue * alphaRange;

            SetImageAlpha(alpha);

            time += Time.deltaTime * pulsateFrequency;

            yield return waitForEndOfFrame;
        }
    }

    private void SetImageAlpha(float alpha)
    {
        if (targetImage == null) return;
        Color color = targetImage.color;
        color.a = alpha;
        targetImage.color = color;
    }
}
