using UnityEngine;
using DG.Tweening;

public class FadeInLightOnEnable : MonoBehaviour
{

    private float myIntensity;
    private Light myLight;
    [SerializeField] private float animationTime = 0.5f;

    private void Awake()
    {
        myLight = GetComponent<Light>();
        myIntensity = myLight.intensity;
        myLight.intensity = 0;
    }

    private void OnEnable()
    {
        myLight.DOIntensity(myIntensity, animationTime);
    }

}
