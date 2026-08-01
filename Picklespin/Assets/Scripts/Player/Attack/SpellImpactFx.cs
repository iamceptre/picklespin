using DG.Tweening;
using FMODUnity;
using UnityEngine;

public class SpellImpactFx : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionFX;
    [SerializeField, Tooltip("the trail the projectile flies with")]
    private ParticleSystem[] flightParticles;
    [SerializeField] private Light spellLight;
    [SerializeField, Tooltip("off = the light is cut the moment the spell lands")]
    private bool fadeOutLight;

    [SerializeField] private StudioEventEmitter explosionSoundEmitter;
    [SerializeField] private StudioEventEmitter explosionReflectionsSoundEmitter;
    [SerializeField] private StudioEventEmitter hitWall;

    private Bullet bullet;
    private CachedCameraMain cachedCameraMain;
    private GameObject explosionFxGameObject;
    private Transform explosionTransform;
    private Color lightColor;

    private void Awake()
    {
        bullet = GetComponent<Bullet>();
        if (explosionFX)
        {
            explosionTransform = explosionFX.transform;
            explosionFxGameObject = explosionFX.gameObject;
        }
        if (spellLight) lightColor = spellLight.color;
    }

    private void Start() => cachedCameraMain = CachedCameraMain.instance;

    public void OnShoot()
    {
        foreach (var ps in flightParticles)
        {
            if (!ps) continue;
            ps.Clear();
            ps.Stop();
            ps.Play();
        }
    }

    public void PlayImpact()
    {
        if (!explosionFxGameObject) return;

        if (cachedCameraMain)
        {
            explosionTransform.position = Vector3.Lerp(transform.position, cachedCameraMain.cachedTransform.position, 0.1f);
        }
        if (explosionFxGameObject.activeSelf) explosionFxGameObject.SetActive(false);
        explosionFxGameObject.SetActive(true);
    }

    public void PlaySounds()
    {
        if (bullet && !bullet.TryClaimImpactSound()) return;

        if (explosionSoundEmitter) explosionSoundEmitter.Play();
        if (explosionReflectionsSoundEmitter) explosionReflectionsSoundEmitter.Play();
        if (hitWall) hitWall.Play();
    }

    public void Shutdown()
    {
        foreach (var ps in flightParticles)
        {
            if (ps) ps.Stop();
        }

        if (!spellLight) return;

        if (!fadeOutLight)
        {
            spellLight.enabled = false;
            return;
        }

        spellLight.DOColor(Color.black, 0.2f).SetUpdate(true).OnComplete(() =>
        {
            spellLight.enabled = false;
            spellLight.color = lightColor;
        });
    }

    public void ResetState()
    {
        if (explosionFxGameObject) explosionFxGameObject.SetActive(false);
        if (!spellLight) return;

        spellLight.DOKill();
        spellLight.color = lightColor;
        spellLight.enabled = true;
    }
}
