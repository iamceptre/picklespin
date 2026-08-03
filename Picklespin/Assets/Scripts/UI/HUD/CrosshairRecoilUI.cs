using UnityEngine;

public class CrosshairRecoilUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform crosshairImage;

    [Header("Scale Settings")]
    [SerializeField] private float recoilAtMinScale = 0f;
    [SerializeField] private float recoilAtMaxScale = 0.1f;
    [SerializeField] private float minScale = 0.1f;
    [SerializeField] private float maxScale = 0.5f;

    [Header("Smoothing")]
    [SerializeField] private float scaleSmoothSpeed = 10f;

    private RecoilMultiplier recoilMultiplier;
    private float lastRecoil = -1f;
    private float targetScale;

    private void Start()
    {
        recoilMultiplier = RecoilMultiplier.instance;

        targetScale = minScale;
        crosshairImage.localScale = new Vector3(minScale, minScale, minScale);
    }

    private void Update()
    {
        float currentRecoil = recoilMultiplier.currentRecoil;

        if (!Mathf.Approximately(currentRecoil, lastRecoil))
        {
            lastRecoil = currentRecoil;
            float t = Mathf.InverseLerp(recoilAtMinScale, recoilAtMaxScale, currentRecoil);
            targetScale = Mathf.Lerp(minScale, maxScale, t);
            targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
        }

        Vector3 currentScale = crosshairImage.localScale;
        Vector3 desiredScale = new(targetScale, targetScale, targetScale);

        crosshairImage.localScale = Vector3.Lerp(
            currentScale,
            desiredScale,
            Time.deltaTime * scaleSmoothSpeed
        );
    }
}
