using UnityEngine;
using FMODUnity;
using UnityEngine.Pool;
using System.Collections;
using DG.Tweening;
public class Bullet : MonoBehaviour
{
    private int originalDamage;

    [Header("Stats")]
    public string spellName;
    [SerializeField] private int damage = 15;
    public int magickaCost = 30;
    public int speed = 60;
    public float myCooldown;
    public float castDuration;
    [SerializeField] float timeBeforeOff = 2;
    [SerializeField] private bool fadeOutLight = false;


    [Header("Assets")]
    [SerializeField] private ParticleSystem explosionFX;
    private GameObject _explosionFxGameObject;
    [SerializeField] private EventReference shootSound;
    public EventReference pullupSound;

    [Header("Special Effects")]
    [SerializeField] private bool doesThisSpellSetOnFire = false;
    [SerializeField] private SetOnFire setOnFire;

    [Header("References")]
    private AiHealth aiHealth;
    private AiVision aiVision;
    private AiHealthUiBar aiHealthUI;
    private CameraShake cameraShake;
    private DamageUI_Spawner damageUiSpawner;
    private GiveExpToPlayer giveExpToPlayer;
    [HideInInspector] public Transform handCastingPoint;
    private MaterialFlashWhenHit flashWhenHit;
    private CachedCameraMain cachedCameraMain;
    private SpellProjectileSpawner spellProjectileSpawner;
    private Ammo ammo;
    private ObjectPool<Bullet> _pool;
    private IEnumerator autoKill;
    private WaitForSeconds autoKillTime;
    [SerializeField] private StudioEventEmitter explosionSoundEmitter;
    private ApplyProjectileForce applyProjectileForce;


    [Header("Misc")]
    [HideInInspector] public bool iWillBeCritical;
    [HideInInspector] public bool hitSomething = false;
    private bool wasLastHitCritical = false;

    [Header("Cache")]
    private Transform _transform;
    private Transform _explosionTransform;
    private Renderer _renderer;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    [SerializeField] [Tooltip("tailParticle")]private ParticleSystem _particleSystem;
    [SerializeField] private Light _light;
    private Color _lightColor;
    private GameObject _gameObject;
    public LightSpell lightSpell;


    void Awake()
    {
        originalDamage = damage;

        _transform = transform;
        _renderer = GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _explosionTransform = explosionFX.transform;
        _explosionFxGameObject = explosionFX.gameObject;
        _lightColor = _light.color;

        autoKillTime = new WaitForSeconds(timeBeforeOff);
        applyProjectileForce = GetComponent<ApplyProjectileForce>();
    }

    private void Start()
    {
        damageUiSpawner = DamageUI_Spawner.instance;
        cameraShake = CameraShake.instance;
        cachedCameraMain = CachedCameraMain.instance;
        ammo = Ammo.instance;
        spellProjectileSpawner = SpellProjectileSpawner.instance;
    }

    private IEnumerator AutoKill()
    {
        yield return autoKillTime;
        ReturnToPool();
    }


    private void OnEnable()
    {
        _explosionFxGameObject.SetActive(false);
        _collider.enabled = true;
        _renderer.enabled = true;
        _rigidbody.isKinematic = false;
        _light.enabled = true;
        autoKill = AutoKill();
        StartCoroutine(autoKill);
    }

    public void OnShoot()
    {
        _particleSystem.Clear();
        _particleSystem.Stop();
        _particleSystem.Play();
        RuntimeManager.PlayOneShot(shootSound);

        if (applyProjectileForce != null)
        {
            applyProjectileForce.Set();
        }
    }

    public void ReturnToPool()
    {
        StopCoroutine(autoKill);
        _pool.Release(this);
    }


    private void OnCollisionEnter(Collision collision)
    {
        hitSomething = true;
        if (collision.transform)
        {
            StopCoroutine(autoKill);
            hitSomething = true;

            if (collision.transform.TryGetComponent<AiReferences>(out AiReferences refs)) //ENEMY HIT REGISTERED
            {

                    aiHealth = refs.Health;
                    aiVision = refs.Vision;
                    aiHealthUI = refs.HpUiBar;
                    giveExpToPlayer = refs.GiveExp;
                    flashWhenHit = refs.MaterialFlash;


                RandomizeCritical();

                if (flashWhenHit != null)
                {
                    flashWhenHit.StopAllCoroutines();


                    if (collision.collider.gameObject.transform.CompareTag("Hitbox_Head"))
                    {
                        Headshot(collision);
                        giveExpToPlayer.wasLastShotAHeadshot = true;
                        flashWhenHit.StartCoroutine(flashWhenHit.FlashHeadshot());
                    }
                    else
                    {
                        giveExpToPlayer.wasLastShotAHeadshot = false;
                        flashWhenHit.StartCoroutine(flashWhenHit.Flash());
                    }

                }

                aiHealth.hp -= damage;

                damageUiSpawner.Spawn(collision.contacts[0].point, damage, wasLastHitCritical);

                if (aiHealth.hp <= 0) {
                    collision.collider.enabled = false;
                    aiHealth.deathEvent.Invoke();
                }
                else
                {
                    ApplySpecialEffect(collision);
                }

                if (aiHealthUI != null)
                {
                    aiHealthUI.RefreshBar();
                }
                HitGetsYouNoticed();

            }

        }
        SpawnExplosion();
        AfterExplosion();
    }


    private void Headshot(Collision collision)
    {
        damage *= 3;
        ParticleSystem headshotParticle = collision.collider.gameObject.transform.GetComponent<ParticleSystem>();
        headshotParticle.Play();
        //maybe a low-key sound
    }


    private void ApplySpecialEffect(Collision collision)
    {
        if (doesThisSpellSetOnFire)
        {
            var addedEffect = collision.gameObject.GetComponent<SetOnFire>();

            if (addedEffect == null)
            {
                addedEffect = collision.gameObject.AddComponent<SetOnFire>();
                addedEffect.effectAudio = setOnFire.effectAudio;
                addedEffect.ParticleObject = setOnFire.ParticleObject;
                addedEffect.killedByBurnEffect = setOnFire.killedByBurnEffect;
                addedEffect.StartFire();
            }
            else
            {
                addedEffect.ResetCountdowns();
            }
        }
    }


    private void RandomizeCritical()
    {
        int criticalTreshold;

        if(ammo.ammo < ammo.maxAmmo * 0.2f) //20% or less mana
        {
            criticalTreshold = 10 - 7; // 7/10 chance of critical
        }
        else
        {
            criticalTreshold = 10 - 1; //WHEN HIGH ON MANA, 1/10 chance of critical
        }

        if (Random.Range(0,10) >= criticalTreshold || iWillBeCritical)
        {
            damage = originalDamage * 4;
            wasLastHitCritical = true;
            RuntimeManager.PlayOneShot("event:/PLACEHOLDER_UNCZ/ohh"); //CRITICAL SOUND
        }
        else
        {
            damage = originalDamage;
            wasLastHitCritical = false;
        }


    }


    private void HitGetsYouNoticed() //make it notice all AIs around
    {
        if (aiVision != null)
        {
            aiVision.hitMeCooldown = 10;
            aiVision.playerJustHitMe = true;
        }
    }

    private void SpawnExplosion()
    {
        _explosionFxGameObject.SetActive(true);
        explosionSoundEmitter.Play();
        _explosionTransform.position = Vector3.Lerp(transform.position, cachedCameraMain.cachedTransform.position, 0.1f);
        cameraShake.ExplosionNearbyShake(Vector3.Distance(transform.position, cachedCameraMain.cachedTransform.position),originalDamage);
    }

    public void AfterExplosion()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        _particleSystem.Clear();
        _particleSystem.Stop();
        _rigidbody.isKinematic = true;

        if (!fadeOutLight)
        {
            _light.enabled = false;
            return;
        }
        _light.DOColor(Color.black, 0.2f).OnComplete(() =>
        {
            _light.enabled = false;
            _light.color = _lightColor;
        });
    }

    public void SetPool(ObjectPool<Bullet> pool)
    {
        _pool = pool;
    }



}