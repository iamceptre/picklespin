using UnityEngine;
using DG.Tweening;

public class SpellHitExplosionAnimation : MonoBehaviour
{
    private Light myLight;

    [SerializeField] private float peakLightIntensity = 3;
    [SerializeField] private float peakLightRange = 45;
    [SerializeField] private float lightFadeOutTime = 1.3f;

    private void Awake()
    {
        myLight = GetComponent<Light>();
    }
    void Start()
    {
        myLight.DOIntensity(peakLightIntensity, 0.07f).OnComplete(FadeOut);
    }


    private void FadeOut()
    {
        myLight.DOIntensity(0, lightFadeOutTime).SetEase(Ease.OutExpo);

        DOTween.To(() => myLight.range, x => myLight.range = x, peakLightRange, 1).SetEase(Ease.OutExpo);
    }



}
