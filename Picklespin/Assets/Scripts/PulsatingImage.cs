using UnityEngine;
using UnityEngine.UI;

public class PulsatingImage : MonoBehaviour
{
    [Header("Image Reference")]
    [SerializeField] private Image targetImage;

    [Header("Pulsating Settings")]
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float pulsateSpeed = 2.0f;

    private bool isPulsating = false;
    private float currentTime = 0f;

    private void Update()
    {
        if (isPulsating && targetImage != null)
        {
            PulsateAlpha();
        }
    }

    private void PulsateAlpha()
    {
        currentTime += Time.deltaTime * pulsateSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(currentTime) + 1f) / 2f);
        Color color = targetImage.color;
        color.a = alpha;
        targetImage.color = color;
    }

    public void StartPulsating()
    {
        isPulsating = true;
        currentTime = 0f;
    }

    public void StopPulsating()
    {
        isPulsating = false;
        if (targetImage != null)
        {
            Color color = targetImage.color;
            color.a = maxAlpha;
            targetImage.color = color;
        }
    }
}