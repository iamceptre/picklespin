using UnityEngine;
using DG.Tweening;

public class SpellHitExplosionAnimation : MonoBehaviour
{
    private Light myLight;
    private float startingRange;
    private Bullet bullet;

    [SerializeField] private float peakLightIntensity = 3;
    [SerializeField] private float peakLightRange = 45;
    [SerializeField] private float lightFadeOutTime = 1.3f;

    private void Awake()
    {
        myLight = GetComponent<Light>();
        startingRange = myLight.range;
        bullet = transform.GetComponentInParent<Bullet>();
    }
    void OnEnable()
    {
        myLight.enabled = true;
        myLight.range = startingRange;
        myLight.DOIntensity(peakLightIntensity, 0.07f).SetEase(Ease.OutExpo).OnComplete(FadeOut);
    }


    private void FadeOut()
    {
        myLight.DOIntensity(0, lightFadeOutTime).SetEase(Ease.OutSine).OnComplete(() =>
        {
            myLight.enabled = false;
            bullet.ReturnToPool();
        });

        DOTween.To(() => myLight.range, x => myLight.range = x, peakLightRange, lightFadeOutTime).SetEase(Ease.OutSine);
    }



}
