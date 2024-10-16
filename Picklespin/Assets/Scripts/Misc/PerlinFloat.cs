using UnityEngine;

public class PerlinFloat : MonoBehaviour
{
    public float floatSpeed = 2.5f;
    public float noiseScale = 0.2f;
    public float noiseStrength = 0.4f;

    private Vector3 initialPosition;
    private Transform cachedTransform;

    void Start()
    {
        if (floatSpeed == 0 || noiseScale == 0 || noiseStrength == 0)
        {
            enabled = false;
            return;
        }

        initialPosition = transform.localPosition;
        cachedTransform = transform;
    }

    void Update()
    {

        float timeFactor = Time.time * floatSpeed;

        float noiseValueX = Mathf.PerlinNoise(timeFactor, 0) * 2.0f - 1.0f;
        float noiseValueY = Mathf.PerlinNoise(0, timeFactor) * 2.0f - 1.0f;
        float noiseValueZ = Mathf.PerlinNoise(timeFactor, timeFactor) * 2.0f - 1.0f;

        Vector3 noiseVector = new Vector3(noiseValueX, noiseValueY, noiseValueZ) * noiseScale * noiseStrength;

        cachedTransform.localPosition = initialPosition + noiseVector;
    }

}