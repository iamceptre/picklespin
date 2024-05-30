using UnityEngine;

public class BulletTurbulence : MonoBehaviour
{
    public float turbulenceIntensity = 1.0f;  // The intensity of the turbulence
    public float turbulenceFrequency = 1.0f;  // The frequency of the turbulence changes
    public int turbulenceUpdateInterval = 2;  // Number of FixedUpdate calls between turbulence updates

    private Rigidbody rb;
    private Vector3 noiseOffset;
    private int updateCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        InitializeNoiseOffset();
    }

    void OnEnable()
    {
        InitializeNoiseOffset();
        updateCounter = 0;
    }

    void InitializeNoiseOffset()
    {
        noiseOffset = new Vector3(
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f)
        );
    }

    void FixedUpdate()
    {
        // Update turbulence every few frames to reduce computational load
        if (updateCounter % turbulenceUpdateInterval == 0)
        {
            ApplyTurbulence();
        }
        updateCounter++;
    }

    void ApplyTurbulence()
    {
        // Generate noise-based turbulence with random offsets
        float time = Time.time * turbulenceFrequency;
        float noiseX = Mathf.PerlinNoise(time + noiseOffset.x, noiseOffset.y) * 2.0f - 1.0f;
        float noiseY = Mathf.PerlinNoise(noiseOffset.x, time + noiseOffset.y) * 2.0f - 1.0f;
        float noiseZ = Mathf.PerlinNoise(time + noiseOffset.z, noiseOffset.z) * 2.0f - 1.0f;

        Vector3 turbulence = new Vector3(noiseX, noiseY, noiseZ) * turbulenceIntensity;

        // Apply the turbulence force to the rigidbody
        rb.AddForce(turbulence, ForceMode.Acceleration);
    }
}