using UnityEngine;
using FMODUnity;
using UnityEngine.Pool;
using System.Collections;
using DG.Tweening;
public class Bullet : MonoBehaviour
{
    private int originalDamage;

    [Header("Stats")]
    [SerializeField] private int spellID;
    public string spellName;
    [SerializeField] private int damage = 15;
    public int magickaCost = 30;
    public int speed = 60;
    public float myCooldown;
    public float castDuration;
    [SerializeField] float timeBeforeOff = 2;
    [SerializeField] private bool fadeOutLight = false;

    [Header("Range Damage")]
    [SerializeField] private bool isRanged = false;
    [SerializeField] private float rangeRadius = 5.0f;
    [SerializeField] private LayerMask detectionLayer;


    [Header("Assets")]
    [SerializeField] private ParticleSystem explosionFX;
    private GameObject _explosionFxGameObject;
    [SerializeField] private EventReference shootSound;
    public EventReference pullupSound;
    [SerializeField] private StudioEventEmitter hitWall;

    [Header("Special Effects")]
    [SerializeField] private bool doesThisSpellSetOnFire = false;

    [Header("References")]
    private AiHealth aiHealth;
    private AiVision aiVision;
    private CameraShakeManagerV2 camShakeManager;
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
    [SerializeField] private StudioEventEmitter explosionReflectionsSoundEmitter;
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
    [SerializeField][Tooltip("tailParticle")] private ParticleSystem[] _particleSystem;
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
        camShakeManager = CameraShakeManagerV2.instance;
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
        for (int i = 0; i < _particleSystem.Length; i++)
        {
            _particleSystem[i].Clear();
            _particleSystem[i].Stop();
            _particleSystem[i].Play();
        }



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

        StopCoroutine(autoKill);
        hitSomething = true;

        SpawnExplosion();
        AfterExplosion();

        if (collision.transform.TryGetComponent(out AiReferences refs)) //direct hit detection
        {
            Collider collider = collision.gameObject.GetComponent<Collider>();
            HitRegistered(collider, refs, collision);
        }
        else //HIT THE WALL 
        {
            PlayExplosionSounds();

            if (hitWall != null)
                hitWall.Play();
        }

        if (isRanged)
        {
            RangeHitDetection(collision);
        }


    }




    private void HitRegistered(Collider collider, AiReferences refs, Collision collision)
    {
        aiHealth = refs.Health;
        aiVision = refs.Vision;
        giveExpToPlayer = refs.GiveExp;
        flashWhenHit = refs.MaterialFlash;

        if (castDuration != 0)
        {
            if (refs.damageTakenBig != null)
                refs.damageTakenBig.Play();
        }
        else
        {
            if (refs.damageTakenSmall != null)
                refs.damageTakenSmall.Play();
        }

        RandomizeCritical(refs);

        flashWhenHit.StopAllCoroutines();


        if (collision.collider.gameObject.transform.CompareTag("Hitbox_Head"))
        {
            Headshot(refs);
            giveExpToPlayer.wasLastShotAHeadshot = true;
            flashWhenHit.FlashHeadshot();
            return;
        }
        else
        {
            giveExpToPlayer.wasLastShotAHeadshot = false;
            flashWhenHit.Flash();
        }

        aiHealth.TakeDamage(damage, false, wasLastHitCritical);
        ApplySpecialEffect(refs);
        HitGetsYouNoticed();
    }


    private void RangeHitDetection(Collision collision)
    {
        Collider[] colliders = Physics.OverlapSphere(collision.GetContact(0).point, rangeRadius, detectionLayer);

        foreach (Collider col in colliders)
        {
            if (col.transform.TryGetComponent(out AiReferences areaRefs))
            {
                if (col != collision.collider) // Avoid double hitting the same object
                {
                    HitRegistered(col, areaRefs, collision);
                }
            }
        }
    }


    private void Headshot(AiReferences refs)
    {
        aiHealth.TakeDamage(damage, true, wasLastHitCritical);
        refs.HeadshotParticle.Play();
        refs.damageTakenEyeshot.Play();
    }


    private void ApplySpecialEffect(AiReferences aiRefs)
    {
        if (aiHealth.hp > 0)
        {
            if (doesThisSpellSetOnFire)
            {
                if (aiRefs.setOnFire != null)
                {
                    aiRefs.setOnFire.enabled = true;
                }
            }
        }
    }



    private void RandomizeCritical(AiReferences refs)
    {
        int criticalTreshold;

        if (ammo.ammo < ammo.maxAmmo * 0.2f) //20% or less mana
        {
            criticalTreshold = 5;
        }
        else
        {
            criticalTreshold = 9;
        }

        if (Random.Range(0, 10) >= criticalTreshold || iWillBeCritical)
        {
            if (refs.damageTakenCritical != null)
            {
                refs.damageTakenCritical.Play();
            }
            damage = (int)(originalDamage * 1.5f);
            wasLastHitCritical = true;

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

        _explosionTransform.position = Vector3.Lerp(transform.position, cachedCameraMain.cachedTransform.position, 0.1f);
        SendShakeSignal();
    }


    private void PlayExplosionSounds()
    {
        explosionSoundEmitter.Play();

        if (explosionReflectionsSoundEmitter != null)
        {
            explosionReflectionsSoundEmitter.Play();
        }

    }

    public void AfterExplosion()
    {
        _collider.enabled = false;
        _renderer.enabled = false;

        for (int i = 0; i < _particleSystem.Length; i++)
        {
            //_particleSystem[i].Clear();
            _particleSystem[i].Stop();
        }


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


    private void SendShakeSignal()
    {
        switch (spellID)
        {
            case 1: //fireball
                camShakeManager.ShakeSelected(8); //add shake multiplier by distance
                break;

            default:

                break;

        }

    }


}