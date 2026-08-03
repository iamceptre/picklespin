using UnityEngine;

public sealed class BulletTurbulence : MonoBehaviour
{
    [SerializeField] private float turbulenceIntensity = 25;
    [SerializeField] private float turbulenceFrequency = 200;

    private const int TurbulenceUpdateInterval = 2;

    private Rigidbody rb;
    private float phaseX;
    private float phaseY;
    private float phaseZ;
    private int updateCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        phaseX = Random.Range(0f, LoopingNoise.Size);
        phaseY = Random.Range(0f, LoopingNoise.Size);
        phaseZ = Random.Range(0f, LoopingNoise.Size);
        updateCounter = 0;
    }

    private void FixedUpdate()
    {
        if (updateCounter % TurbulenceUpdateInterval == 0) ApplyTurbulence();
        updateCounter++;
    }

    private void ApplyTurbulence()
    {
        float walk = Time.time * turbulenceFrequency;

        Vector3 turbulence = new(
            LoopingNoise.Sample(walk + phaseX),
            LoopingNoise.Sample(walk + phaseY),
            LoopingNoise.Sample(walk + phaseZ)
        );

        rb.AddForce(turbulence * turbulenceIntensity, ForceMode.Acceleration);
    }
}
