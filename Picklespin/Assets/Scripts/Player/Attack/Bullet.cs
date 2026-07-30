using UnityEngine;
using FMODUnity;
using UnityEngine.Pool;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    private int originalDamage;

    [SerializeField] private int spellID;
    public string spellName;
    [SerializeField] private int damage = 15;
    [SerializeField, Tooltip("scale damage by player speed (×0.25 standing – ×2 at bhop speeds); off = flat damage")]
    private bool useSpeedDamageMultiplier = true;
    public int magickaCost = 30;
    public int speed = 60;
    public float myCooldown;
    public float castDuration;
    [SerializeField] private float timeBeforeOff = 2f;
    [SerializeField] private bool fadeOutLight = false;

    [SerializeField] private bool isRanged = false;
    [SerializeField] private float rangeRadius = 5f;
    [SerializeField] private LayerMask detectionLayer;

    [SerializeField] private ParticleSystem explosionFX;
    private GameObject _explosionFxGameObject;
    [SerializeField] private EventReference shootSound;
    public EventReference pullupSound;
    [SerializeField] private StudioEventEmitter hitWall;

    [SerializeField] private bool doesThisSpellSetOnFire = false;

    private PlayerHP playerHP;
    private CameraShakeManagerV2 camShakeManager;
    [HideInInspector] public Transform handCastingPoint;
    private CachedCameraMain cachedCameraMain;
    private Ammo ammo;
    private ObjectPool<Bullet> _pool;
    private IEnumerator autoKill;
    private WaitForSeconds autoKillTime;
    [SerializeField] private StudioEventEmitter explosionSoundEmitter;
    [SerializeField] private StudioEventEmitter explosionReflectionsSoundEmitter;
    private ApplyProjectileForce applyProjectileForce;

    [HideInInspector] public bool iWillBeCritical;
    [HideInInspector] public bool hitSomething;
    private bool released;
    private bool wasLastHitCritical;
    private bool alreadyPlayedExplosionSound;

    private Transform _explosionTransform;
    private Renderer _renderer;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;
    [SerializeField] private ParticleSystem[] _particleSystem;
    [SerializeField] private Light _light;
    private Color _lightColor;
    public LightSpell lightSpell;
    private static readonly Collider[] overlapResults = new Collider[32];
    private static readonly Collider[] rocketJumpResults = new Collider[8];
    private static readonly HashSet<AiReferences> areaHitBuffer = new();

    [SerializeField] private float rocketJumpForce = 50f;
    [SerializeField] private float rocketJumpUpwardsModifier = 1f;

    [SerializeField] private LayerMask decalLayerMask;

    private void Awake()
    {
        originalDamage = damage;
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
        camShakeManager = CameraShakeManagerV2.instance;
        cachedCameraMain = CachedCameraMain.instance;
        playerHP = PlayerHP.Instance;
        ammo = Ammo.instance;
    }

    private void OnEnable()
    {
        ResetBulletState();
        autoKill = AutoKill();
        StartCoroutine(autoKill);
    }

    private IEnumerator AutoKill()
    {
        yield return autoKillTime;
        ReturnToPool();
    }

    public void OnShoot()
    {
        alreadyPlayedExplosionSound = false;
        foreach (var ps in _particleSystem)
        {
            ps.Clear();
            ps.Stop();
            ps.Play();
        }
        RuntimeManager.PlayOneShot(shootSound);
        if (applyProjectileForce) applyProjectileForce.Set();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (hitSomething) return;
        if (collider.CompareTag("Hitbox_Head"))
        {
            hitSomething = true;
            GeneralAfterHit(collider, true);
        }
        else if (collider.CompareTag("NPC_Hitbox"))
        {
            hitSomething = true;
            GeneralAfterHit(collider, false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hitSomething) return;
        hitSomething = true;
        StopCoroutine(autoKill);
        if (isRanged) RangeHitDetection(collision.collider, collision.GetContact(0).point);
        var collisionCollider = collision.collider;
        if (collisionCollider.TryGetComponent(out AiReferences refs))
        {
            HitRegistered(refs, false);
        }
        else
        {
            PlayExplosionSounds();
            AttemptSpawnDecal(collision);
        }
        SpawnExplosion();
        ApplyRocketJumpForce(collision.GetContact(0).point);
        AfterExplosion();
    }

    private void GeneralAfterHit(Collider collider, bool weakPointHit)
    {
        StopCoroutine(autoKill);
        if (isRanged) RangeHitDetection(collider, collider.transform.position);
        var grandparentTransform = collider.transform.parent != null ? collider.transform.parent.parent : null;
        if (grandparentTransform != null && grandparentTransform.TryGetComponent(out AiReferences refs))
        {
            HitRegistered(refs, weakPointHit);
        }
        else
        {
            PlayExplosionSounds();
        }
        SpawnExplosion();
        AfterExplosion();
    }

    // Every part of an enemy is optional (see AiReferences), so nothing in the
    // damage path may assume one exists.
    private void HitRegistered(AiReferences refs, bool weakPointHit)
    {
        _collider.enabled = false;
        if (!refs.Health) return;

        if (castDuration != 0)
        {
            if (refs.damageTakenBig && !alreadyPlayedExplosionSound)
            {
                SetCriticalToNo();
                alreadyPlayedExplosionSound = true;
                refs.damageTakenBig.Play();
            }
        }
        else
        {
            if (refs.damageTakenSmall)
            {
                RandomizeCritical(refs);
                refs.damageTakenSmall.Play();
            }
        }

        // read before the hit lands: the death chain fires inside TakeDamage and
        // GiveExp reads this flag to decide the headshot bonus
        if (refs.GiveExp) refs.GiveExp.wasLastShotAHeadshot = weakPointHit;

        if (refs.MaterialFlash)
        {
            if (weakPointHit) refs.MaterialFlash.FlashHeadshot();
            else refs.MaterialFlash.Flash();
        }

        if (refs.Vision) refs.Vision.HitShowsMePlayer();

        if (weakPointHit) Headshot(refs);
        else refs.Health.TakeDamage(SpeedScaledDamage(), false, wasLastHitCritical);

        ApplySpecialEffect(refs);
    }

    // Splash damage around the impact. The direct hit is excluded (it is handled
    // by the caller) and each enemy is only registered once no matter how many
    // of its colliders the sphere catches.
    private void RangeHitDetection(Collider directHit, Vector3 center)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(center, rangeRadius, overlapResults, detectionLayer);
        areaHitBuffer.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            var col = overlapResults[i];
            if (col == null || col == directHit) continue;
            if (!col.transform.TryGetComponent(out AiReferences areaRefs)) continue;
            if (!areaHitBuffer.Add(areaRefs)) continue;
            if (areaRefs.Health && !areaRefs.Health.IsAlive) continue; // already dissolving

            HitRegistered(areaRefs, false);
        }
    }

    private void Headshot(AiReferences refs)
    {
        refs.Health.TakeDamage(SpeedScaledDamage(), true, wasLastHitCritical);
        if (refs.HeadshotParticle) refs.HeadshotParticle.Play();
        if (refs.damageTakenEyeshot) refs.damageTakenEyeshot.Play();
    }

    // faster player = harder hits: ×0.25 standing, up to ×2 at bhop/rocket-jump speeds
    private int SpeedScaledDamage()
    {
        if (!useSpeedDamageMultiplier) return damage;
        float multiplier = PlayerMovement.Instance ? PlayerMovement.Instance.SpeedDamageMultiplier : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }

    // Runs after the impact damage. A hit that already killed must not light the
    // corpse on fire — low-HP enemies get finished off by the fireball instead of
    // burning, and the burn must never restart a death chain that is underway.
    private void ApplySpecialEffect(AiReferences aiRefs)
    {
        // no SetOnFire on this enemy means it simply cannot be lit
        if (!doesThisSpellSetOnFire || !aiRefs.setOnFire) return;
        if (!aiRefs.Health || !aiRefs.Health.IsAlive || aiRefs.Health.hp <= 0) return;

        aiRefs.setOnFire.Ignite();
    }

    private void RandomizeCritical(AiReferences refs)
    {
        int criticalThreshold = (ammo.ammo < ammo.maxAmmo * 0.2f) ? 5 : 9;
        if (Random.Range(0, 10) >= criticalThreshold || iWillBeCritical)
        {
            if (refs.damageTakenCritical) refs.damageTakenCritical.Play();
            damage = (int)(originalDamage * PhiMath.PHI); // crits hit φ× harder
            wasLastHitCritical = true;
        }
        else
        {
            SetCriticalToNo();
        }
    }

    private void SetCriticalToNo()
    {
        damage = originalDamage;
        wasLastHitCritical = false;
    }

    private void SpawnExplosion()
    {
        // position first, then force a fresh enable. SpellHitExplosionAnimation
        // drives its flash — and this bullet's trip back to the pool — from
        // OnEnable, and SetActive(true) on an already-active object does not
        // re-run it: the bullet would be stranded with its light still on.
        _explosionTransform.position = Vector3.Lerp(transform.position, cachedCameraMain.cachedTransform.position, 0.1f);
        if (_explosionFxGameObject.activeSelf) _explosionFxGameObject.SetActive(false);
        _explosionFxGameObject.SetActive(true);
        SendShakeSignal();
    }

    private void PlayExplosionSounds()
    {
        if (alreadyPlayedExplosionSound) return;
        alreadyPlayedExplosionSound = true;
        explosionSoundEmitter.Play();
        if (explosionReflectionsSoundEmitter) explosionReflectionsSoundEmitter.Play();
        if (hitWall) hitWall.Play();
    }

    private void ApplyRocketJumpForce(Vector3 explosionCenter)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(explosionCenter, rangeRadius, rocketJumpResults, detectionLayer, QueryTriggerInteraction.Ignore);
        bool characterControllerFound = false;

        for (int i = 0; i < hitCount; i++)
        {
            if (characterControllerFound) break;

            var col = rocketJumpResults[i];
            if (col == null) continue;

            var rb = col.attachedRigidbody;
            if (rb && !rb.isKinematic)
            {
                rb.AddExplosionForce(rocketJumpForce, explosionCenter, rangeRadius, rocketJumpUpwardsModifier, ForceMode.Impulse);
            }
            else
            {
                var cc = col.GetComponent<CharacterController>();
                if (cc)
                {
                    var playerMove = cc.GetComponent<PlayerMovement>();
                    if (playerMove)
                    {
                        playerMove.AddExplosionJump(rocketJumpForce * 2f, explosionCenter, rangeRadius);
                        var distance = Vector3.Distance(playerMove.transform.position, explosionCenter);
                        var proximityFactor = 1f - distance / rangeRadius;
                        proximityFactor = Mathf.Clamp01(proximityFactor);
                        playerHP.ModifyHP(Mathf.RoundToInt(rocketJumpForce * proximityFactor) * -2);

                        characterControllerFound = true;
                    }
                }
            }
        }
    }

    public void AfterExplosion()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        foreach (var ps in _particleSystem) ps.Stop();
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

    // Several things can decide a bullet is finished — the auto-kill timer, the
    // explosion light finishing its fade, the light spell being superseded. The
    // pool is built with collectionCheck off, so a double release would silently
    // put the same bullet in twice and two shots would then share one object.
    public void ReturnToPool()
    {
        if (released) return;
        released = true;

        if (autoKill != null) StopCoroutine(autoKill);
        _pool.Release(this);
    }

    public void SetPool(ObjectPool<Bullet> pool)
    {
        _pool = pool;
    }

    private void SendShakeSignal()
    {
        if (spellID == 1) camShakeManager.ShakeSelected(8);
    }

    private void AttemptSpawnDecal(Collision collision) //static objects
    {
        if (!collision.gameObject.isStatic) return;

        var contact = collision.GetContact(0);
        var hitObject = collision.collider.gameObject;

        if (((1 << hitObject.layer) & decalLayerMask) == 0) return;

        string hitTag = hitObject.tag;

        if (SpellDecalManager.Instance != null)
        {
            SpellDecalManager.Instance.SpawnDecal(contact.point + contact.normal * 0.01f,Quaternion.LookRotation(contact.normal),spellID,hitTag.GetHashCode());
        }
    }

    private void ResetBulletState()
    {
        _explosionFxGameObject.SetActive(false);
        _collider.enabled = true;
        _renderer.enabled = true;
        _rigidbody.isKinematic = false;
        _light.enabled = true;
        hitSomething = false;
        released = false;
    }
}