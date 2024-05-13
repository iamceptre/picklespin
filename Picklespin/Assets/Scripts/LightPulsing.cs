using DG.Tweening;
using UnityEngine;

public class LightPulse : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lowestIntensity = 0;

    private Light lightSource;
    private float lightIntensity;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        lightIntensity = lightSource.intensity;
    }
    void Start()
    {
        lightSource = GetComponent<Light>();
        DOTween.To(() => lightSource.intensity, x => lightSource.intensity = x, lowestIntensity, speed).SetLoops(-1, LoopType.Yoyo);
    }

}