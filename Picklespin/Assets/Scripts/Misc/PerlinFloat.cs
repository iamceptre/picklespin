using UnityEngine;

public class PerlinFloat : MonoBehaviour
{
    public float floatSpeed = 2;
    public float noiseScale = 0.0f;
    public float noiseStrength = 1;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        float x = Mathf.PerlinNoise(Time.time * floatSpeed, 0) * 2.0f - 1.0f;
        float y = Mathf.PerlinNoise(0, Time.time * floatSpeed) * 2.0f - 1.0f;
        float z = Mathf.PerlinNoise(Time.time * floatSpeed, Time.time * floatSpeed) * 2.0f - 1.0f;

        Vector3 noiseVector = new Vector3(x, y, z) * noiseScale * noiseStrength;

        transform.position = initialPosition + noiseVector;
    }
}