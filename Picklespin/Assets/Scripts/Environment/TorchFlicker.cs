using UnityEngine;

[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    [HideInInspector] public Transform cachedTransform;
    [HideInInspector] public bool isCulled;

    [HideInInspector] public Vector3 initialLocalPosition;
    [HideInInspector] public Light torchLight;
    [HideInInspector] public float initialIntensity;

    // keeps the period of the old 360-step wavetable so scene-tuned speeds behave the same
    private const float TimeScale = Mathf.PI * 2f / 360f;

    private float positionTime;
    private float intensityTime;
    private float phase;
    private float speedMultiplier;
    private float intensitySpeedMultiplier;

    private void Awake()
    {
        cachedTransform = transform;
        torchLight = GetComponent<Light>();
        initialLocalPosition = cachedTransform.localPosition;
        initialIntensity = torchLight.intensity;
    }

    private void Start()
    {
        TorchFlickerManager.instance?.RegisterTorch(this);
    }

    private void OnDisable()
    {
        TorchFlickerManager.instance?.UnregisterTorch(this);
    }

    public void Initialize()
    {
        phase = Random.Range(0f, Mathf.PI * 2f);
        positionTime = Random.Range(0f, Mathf.PI * 2f);
        intensityTime = Random.Range(0f, Mathf.PI * 2f);
        speedMultiplier = Random.Range(0.8f, 1.2f);
        intensitySpeedMultiplier = Random.Range(0.8f, 1.2f);
    }

    public void ResetFlicker()
    {
        cachedTransform.localPosition = initialLocalPosition;
        torchLight.intensity = initialIntensity;
    }

    // Two sines with frequencies in ratio φ (the most irrational number): the sum is
    // quasi-periodic — it never repeats, yet stays perfectly smooth. Organic fire from
    // four sin() calls; no wavetables, no noise textures, no allocations.
    public void FlickerUpdate(float deltaTime, float baseFlickerSpeed, float flickerAmplitude, float minLightIntensity, float baseIntensitySpeed)
    {
        if (isCulled) return;

        positionTime += deltaTime * baseFlickerSpeed * speedMultiplier * TimeScale;
        intensityTime += deltaTime * baseIntensitySpeed * intensitySpeedMultiplier * TimeScale;

        float swayA = Mathf.Sin(positionTime + phase);
        float swayB = Mathf.Sin(positionTime * PhiMath.PHI);
        cachedTransform.localPosition = initialLocalPosition + new Vector3(
            swayA,
            swayA * swayB * PhiMath.INV_PHI, // product term adds a third harmonic for free
            swayB) * flickerAmplitude;

        float glowA = Mathf.Sin(intensityTime + phase);
        float glowB = Mathf.Sin(intensityTime * PhiMath.PHI + phase);
        float wave = 0.5f + 0.25f * (glowA + glowB); // 0..1, hovers mid-range like real flame
        float targetIntensity = Mathf.Lerp(minLightIntensity, initialIntensity, wave);

        if (Mathf.Abs(torchLight.intensity - targetIntensity) > 0.005f)
        {
            torchLight.intensity = targetIntensity;
        }
    }
}
