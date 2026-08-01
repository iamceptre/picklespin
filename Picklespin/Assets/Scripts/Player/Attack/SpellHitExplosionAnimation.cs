using UnityEngine;
using DG.Tweening;

public class SpellHitExplosionAnimation : MonoBehaviour
{
    private Light myLight;
    private float startingRange;
    private float startingIntensity;
    private Bullet bullet;

    [SerializeField] private float peakLightIntensity = 3;
    [SerializeField] private float peakLightRange = 45;
    [SerializeField] private float lightFadeOutTime = 1.3f;

    private void Awake()
    {
        myLight = GetComponent<Light>();
        startingRange = myLight.range;
        startingIntensity = myLight.intensity;
        bullet = GetComponentInParent<Bullet>();
    }

    void OnEnable()
    {
        KillTweens();
        myLight.range = startingRange;
        myLight.intensity = startingIntensity;
        myLight.enabled = true;

        myLight.DOIntensity(peakLightIntensity, 0.07f)
            .SetEase(Ease.OutExpo)
            .SetTarget(myLight)
            .SetUpdate(true)
            .OnComplete(FadeOut);
    }

    private void OnDisable()
    {
        KillTweens();
        myLight.enabled = false;
    }

    private void FadeOut()
    {
        myLight.DOIntensity(0, lightFadeOutTime)
            .SetEase(Ease.OutSine)
            .SetTarget(myLight)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                myLight.enabled = false;
                if (bullet) bullet.ReturnToPool();
            });

        DOTween.To(() => myLight.range, x => myLight.range = x, peakLightRange, lightFadeOutTime)
            .SetEase(Ease.OutSine)
            .SetTarget(myLight)
            .SetUpdate(true);
    }

    private void KillTweens()
    {
        myLight.DOKill();
        DOTween.Kill(myLight);
    }
}
