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
    [SerializeField, Tooltip("Umbral's shell: the splash only bursts while the shared bar is over half")]
    private bool aoeRequiresChargedBar = false;
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

    // per-bullet, not static like the buffers above: several pierced shots fly at once
    private readonly HashSet<AiReferences> piercedThisFlight = new();
    private Vector3 pierceVelocity;
    private bool convertedThisFlight;

    // sampled when the shot was fired, not when it lands
    private float flightDamageMultiplier = 1f;

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
        pierceVelocity = _rigidbody.linearVelocity;
        flightDamageMultiplier = PlayerClasses.FlightDamageMultiplier;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (hitSomething) return;

        bool weakPointHit = collider.CompareTag("Hitbox_Head");
        if (!weakPointHit && !collider.CompareTag("NPC_Hitbox")) return;

        if (PlayerClasses.PiercingProjectiles)
        {
            Pierce(ResolveHitboxOwner(collider), weakPointHit);
            return;
        }

        hitSomething = true;
        GeneralAfterHit(collider, weakPointHit);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hitSomething) return;

        if (PlayerClasses.PiercingProjectiles && collision.collider.TryGetComponent(out AiReferences pierceTarget))
        {
            Pierce(pierceTarget, false);
            return;
        }

        hitSomething = true;
        StopCoroutine(autoKill);
        Vector3 impactPoint = collision.GetContact(0).point;
        if (isRanged) RangeHitDetection(collision.collider, impactPoint);
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
        CommandAlliesIfLightSpell(impactPoint);
        ApplyRocketJumpForce(impactPoint);
        AfterExplosion();
    }

    private void GeneralAfterHit(Collider collider, bool weakPointHit)
    {
        StopCoroutine(autoKill);
        if (isRanged) RangeHitDetection(collider, collider.transform.position);
        AiReferences refs = ResolveHitboxOwner(collider);
        if (refs) HitRegistered(refs, weakPointHit);
        else PlayExplosionSounds();
        SpawnExplosion();
        CommandAlliesIfLightSpell(collider.transform.position);
        AfterExplosion();
    }

    // hitboxes hang two levels under the enemy root, which is where AiReferences lives
    private static AiReferences ResolveHitboxOwner(Collider collider)
    {
        Transform grandparent = collider.transform.parent != null ? collider.transform.parent.parent : null;
        return grandparent != null && grandparent.TryGetComponent(out AiReferences refs) ? refs : null;
    }

    private void Pierce(AiReferences refs, bool weakPointHit)
    {
        if (refs != null && piercedThisFlight.Add(refs) && refs.Health && refs.Health.IsAlive)
        {
            HitRegistered(refs, weakPointHit, keepFlying: true);
        }

        // a bounce off the enemy's collider would send the shot anywhere but forward
        if (pierceVelocity.sqrMagnitude > 0f) _rigidbody.linearVelocity = pierceVelocity;
    }

    private void HitRegistered(AiReferences refs, bool weakPointHit, bool keepFlying = false)
    {
        if (!keepFlying) _collider.enabled = false;
        if (!refs.Health) return;

        if (lightSpell && PlayerClasses.LightSpellConverts)
        {
            convertedThisFlight = true;
            ConvertedAlly.Convert(refs);
            return;
        }

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

    private void RangeHitDetection(Collider directHit, Vector3 center)
    {
        if (aoeRequiresChargedBar && !PlayerClasses.ChargedBarReady) return;

        int hitCount = Physics.OverlapSphereNonAlloc(center, rangeRadius, overlapResults, detectionLayer);
        areaHitBuffer.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            var col = overlapResults[i];
            if (col == null || col == directHit) continue;
            if (!col.transform.TryGetComponent(out AiReferences areaRefs)) continue;
            if (!areaHitBuffer.Add(areaRefs)) continue;
            if (areaRefs.Health && !areaRefs.Health.IsAlive) continue;

            HitRegistered(areaRefs, false);
        }
    }

    private void Headshot(AiReferences refs)
    {
        refs.Health.TakeDamage(SpeedScaledDamage(), true, wasLastHitCritical);
        if (refs.HeadshotParticle) refs.HeadshotParticle.Play();
        if (refs.damageTakenEyeshot) refs.damageTakenEyeshot.Play();
    }

    // multipliers are read here, never written into the prefab's damage field - the
    // Editor would keep that between sessions
    private int SpeedScaledDamage()
    {
        float multiplier = WishUpgrades.SpellDamageMultiplier(spellName)
                           * PlayerClasses.ProjectileDamageMultiplier
                           * flightDamageMultiplier;
        if (useSpeedDamageMultiplier && PlayerClasses.SpeedDamageActive && PlayerMovement.Instance)
        {
            multiplier *= PlayerMovement.Instance.SpeedDamageMultiplier;
        }
        return Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }

    private void CommandAlliesIfLightSpell(Vector3 point)
    {
        if (lightSpell && PlayerClasses.LightSpellConverts && !convertedThisFlight)
        {
            ConvertedAlly.CommandAll(point);
        }
    }

    // runs after the impact damage: a hit that already killed must not light the
    // corpse, or the burn would restart a death chain already underway
    private void ApplySpecialEffect(AiReferences aiRefs)
    {
        if (!doesThisSpellSetOnFire || !aiRefs.setOnFire) return;
        if (!aiRefs.Health || !aiRefs.Health.IsAlive || aiRefs.Health.hp <= 0) return;

        aiRefs.setOnFire.Ignite();
    }

    private void RandomizeCritical(AiReferences refs)
    {
        int criticalThreshold = ammo.IsLow ? 5 : 9;
        if (Random.Range(0, 10) >= criticalThreshold || iWillBeCritical)
        {
            if (refs.damageTakenCritical) refs.damageTakenCritical.Play();
            damage = (int)(originalDamage * PhiMath.PHI);
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
        // SpellHitExplosionAnimation drives the flash and this bullet's trip back to
        // the pool from OnEnable, and SetActive(true) on an active object won't re-run it
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
        // the wish and Blastfool buff the push only — self-damage stays keyed to the base force
        float boostedForce = rocketJumpForce * WishUpgrades.RocketJumpForceMultiplier * PlayerClasses.RocketJumpForceMultiplier;

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
                        characterControllerFound = true;

                        var distance = Vector3.Distance(playerMove.transform.position, explosionCenter);
                        var proximityFactor = Mathf.Clamp01(1f - distance / rangeRadius);

                        if (proximityFactor < PlayerClasses.RocketJumpMinProximity) break;

                        playerMove.AddExplosionJump(boostedForce * 2f, explosionCenter, rangeRadius);
                        if (WishUpgrades.RocketJumpSelfDamage)
                        {
                            float selfDamage = rocketJumpForce * proximityFactor * PlayerClasses.RocketJumpSelfDamageMultiplier;
                            playerHP.ModifyHP(Mathf.RoundToInt(selfDamage) * -2);
                        }
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

    // several things end a bullet (timer, light fade, a superseded light spell) and
    // the pool runs with collectionCheck off, so a double release must be guarded here
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

    private void AttemptSpawnDecal(Collision collision)
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
        piercedThisFlight.Clear();
        pierceVelocity = Vector3.zero;
        convertedThisFlight = false;
        flightDamageMultiplier = 1f;
    }
}