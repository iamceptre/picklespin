using UnityEngine;

public class LightFluctuation : MonoBehaviour
{
    [SerializeField] private float minimumInstensity = 0.5f;
    [SerializeField] private float speed = 1f;

    private Light lightSource;
    private float startingInstensity;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        startingInstensity = lightSource.intensity;
    }

    void Update()
    {
        lightSource.intensity = Mathf.Lerp(minimumInstensity, startingInstensity, Mathf.PerlinNoise(Time.time * speed, 0f));
    }
}