using UnityEngine;
using DG.Tweening;

// The impact flash on a spell's explosion FX. It also owns the projectile's
// return to the pool: the bullet goes back once this light has finished fading.
//
// That makes the tween chain load-bearing — if it is ever interrupted the light
// stays lit at full intensity and the bullet never comes back, which is how
// stray Spell_Bullet_Netherlight(Clone) lights ended up parked around the level
// forever. OnEnable therefore re-arms from a known state and OnDisable puts the
// light out unconditionally, so no state can survive a trip through the pool.
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
            .OnComplete(() =>
            {
                myLight.enabled = false;
                if (bullet) bullet.ReturnToPool();
            });

        // SetTarget so DOKill on the light reaches this one too — it has no
        // implicit target of its own and would otherwise outlive the light
        DOTween.To(() => myLight.range, x => myLight.range = x, peakLightRange, lightFadeOutTime)
            .SetEase(Ease.OutSine)
            .SetTarget(myLight);
    }

    private void KillTweens()
    {
        myLight.DOKill();
        DOTween.Kill(myLight);
    }
}
