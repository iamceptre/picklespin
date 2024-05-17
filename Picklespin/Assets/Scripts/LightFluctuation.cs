using UnityEngine;

public class LightFluctuation : MonoBehaviour
{
    public Vector2 intensityRange = new Vector2(0.5f, 1);
    public float speed = 1f;

    private Light lightSource;

    void Start()
    {
        lightSource = GetComponent<Light>();
    }

    void Update()
    {
        lightSource.intensity = Mathf.Lerp(intensityRange.x, intensityRange.y, Mathf.PerlinNoise(Time.time * speed, 0f));
    }
}