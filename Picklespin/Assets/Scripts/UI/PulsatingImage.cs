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

    // Pre-calculated values
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

    public void StartPulsating()
    {
        if (isPulsating) return;
        isPulsating = true;
        StopCoroutine(pulsateEnumerator);
        StartCoroutine(pulsateEnumerator);
    }

    public void StopPulsating()
    {
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

            // Precompute normalized sinusoidal value
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
