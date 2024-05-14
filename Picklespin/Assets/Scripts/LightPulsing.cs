using DG.Tweening;
using UnityEngine;

public class LightPulse : MonoBehaviour
{
    [SerializeField] private float animationTime = 1f;
    [SerializeField] private float lowestIntensity;

    private Light lightSource;
    private float startLightIntensity;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        startLightIntensity = lightSource.intensity;
    }
    void Start()
    {
        lightSource = GetComponent<Light>();
        lightSource.intensity = 0;
        lightSource.DOIntensity(startLightIntensity, 0.5f).OnComplete(Pulse);
    }

    private void Pulse()
    {
        lightSource.DOIntensity(lowestIntensity, animationTime).SetLoops(-1, LoopType.Yoyo);
    }


}