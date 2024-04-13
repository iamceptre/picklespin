using UnityEngine;

public class PerlinFloat : MonoBehaviour
{
    public float floatSpeed = 1.0f; // Speed of floating
    public float noiseScale = 1.0f; // Scale of Perlin noise
    public float noiseStrength = 1.0f; // Strength of Perlin noise

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        // Generate Perlin noise values
        float x = Mathf.PerlinNoise(Time.time * floatSpeed, 0) * 2.0f - 1.0f;
        float y = Mathf.PerlinNoise(0, Time.time * floatSpeed) * 2.0f - 1.0f;
        float z = Mathf.PerlinNoise(Time.time * floatSpeed, Time.time * floatSpeed) * 2.0f - 1.0f;

        // Scale and adjust strength of noise
        Vector3 noiseVector = new Vector3(x, y, z) * noiseScale * noiseStrength;

        // Apply noise to the initial position
        transform.position = initialPosition + noiseVector;
    }
}