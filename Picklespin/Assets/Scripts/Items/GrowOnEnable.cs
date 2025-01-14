using UnityEngine;

public class GrowOnEnable : MonoBehaviour
{
    private const float growDuration = 0.3f;
    private Vector3 originalScale;
    private float inverseDuration;
    private float elapsedTime;
    private bool isGrowing;

    private void Awake()
    {
        originalScale = transform.localScale;
        inverseDuration = 1f / growDuration;
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        elapsedTime = 0f;
        isGrowing = true;
    }

    private void Update()
    {
        if (!isGrowing) return;

        elapsedTime += Time.deltaTime;
        float t = elapsedTime * inverseDuration;

        if (t >= 1f)
        {
            transform.localScale = originalScale;
            isGrowing = false;
            return;
        }

        transform.localScale = originalScale * EaseInOutSine(t);
    }

    private float EaseInOutSine(float x)
    {
        return -0.5f * (Mathf.Cos(Mathf.PI * x) - 1f);
    }
}
