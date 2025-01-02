using UnityEngine;

[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    [HideInInspector] public Transform cachedTransform;
    [HideInInspector] public bool isCulled;

    [HideInInspector] public Vector3 initialLocalPosition;
    [HideInInspector] public Light torchLight;
    [HideInInspector] public float initialIntensity;

    private int precomputeSteps;
    private float waveTimeTransform;
    private float waveTimeIntensity;

    private float[] precomputedSin;
    private float[] precomputedCos;

    private float speedMultiplier;
    private float intensitySpeedMultiplier;

    private void Awake()
    {
        cachedTransform = transform;
        torchLight = GetComponent<Light>();
        initialLocalPosition = cachedTransform.localPosition;
        initialIntensity = torchLight.intensity;

        speedMultiplier = Random.Range(0.8f, 1.2f);
        intensitySpeedMultiplier = Random.Range(0.8f, 1.2f);
    }

    private void Start()
    {
        TorchFlickerManager.instance?.RegisterTorch(this);
    }

    private void OnDisable()
    {
        TorchFlickerManager.instance?.UnregisterTorch(this);
    }

    public void Initialize(int precomputeSteps, float[] precomputedSin, float[] precomputedCos)
    {
        this.precomputeSteps = precomputeSteps;
        this.precomputedSin = precomputedSin;
        this.precomputedCos = precomputedCos;

        waveTimeTransform = Random.Range(0, precomputeSteps);
        waveTimeIntensity = Random.Range(0, precomputeSteps);
    }

    public void ResetFlicker()
    {
        cachedTransform.localPosition = initialLocalPosition;
        torchLight.intensity = initialIntensity;
        waveTimeTransform = 0f;
        waveTimeIntensity = 0f;
    }

    public void FlickerUpdate(float deltaTime, float baseFlickerSpeed, float flickerAmplitude, float minLightIntensity, float baseIntensitySpeed)
    {
        if (isCulled) return;

        waveTimeTransform = (waveTimeTransform + deltaTime * baseFlickerSpeed * speedMultiplier) % precomputeSteps;
        waveTimeIntensity = (waveTimeIntensity + deltaTime * baseIntensitySpeed * intensitySpeedMultiplier) % precomputeSteps;

        int waveTransformIndex = Mathf.FloorToInt(waveTimeTransform);
        int waveIntensityIndex = Mathf.FloorToInt(waveTimeIntensity);

        Vector3 flickerOffset = new Vector3(
            precomputedSin[waveTransformIndex],
            precomputedCos[(waveTransformIndex + 45) % precomputeSteps],
            precomputedSin[(waveTransformIndex + 90) % precomputeSteps]
        ) * flickerAmplitude;

        cachedTransform.localPosition = Vector3.Lerp(cachedTransform.localPosition, initialLocalPosition + flickerOffset, 0.2f);


        float targetIntensity = Mathf.Lerp(
            minLightIntensity,
            initialIntensity,
            (precomputedSin[waveIntensityIndex] + 1f) * 0.5f
        );

        if (Mathf.Abs(torchLight.intensity - targetIntensity) > 0.01f)
        {
            torchLight.intensity = Mathf.Lerp(torchLight.intensity, targetIntensity, 0.2f);
        }
    }
}
