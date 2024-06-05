using UnityEngine;

public class BulletTurbulence : MonoBehaviour
{
    [SerializeField] private float turbulenceIntensity = 25;
    [SerializeField] private float turbulenceFrequency = 200;
     private int turbulenceUpdateInterval = 2;

    private Vector3 _turbulence;

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
        _turbulence = Vector3.zero;
        noiseOffset = new Vector3(
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f)
        );
    }

    void FixedUpdate()
    {
        if (updateCounter % turbulenceUpdateInterval == 0)
        {
            ApplyTurbulence();
        }
        updateCounter++;
    }

    void ApplyTurbulence()
    {
        float time = Time.time * turbulenceFrequency;
        float noiseX = Mathf.PerlinNoise(time + noiseOffset.x, noiseOffset.y) * 2.0f - 1.0f;
        float noiseY = Mathf.PerlinNoise(noiseOffset.x, time + noiseOffset.y) * 2.0f - 1.0f;
        float noiseZ = Mathf.PerlinNoise(time + noiseOffset.z, noiseOffset.z) * 2.0f - 1.0f;

        _turbulence = new Vector3(noiseX, noiseY, noiseZ) * turbulenceIntensity;

        rb.AddForce(_turbulence, ForceMode.Acceleration);
    }
}