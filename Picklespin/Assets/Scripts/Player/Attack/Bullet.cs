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

    private AiHealth aiHealth;
    private AiVision aiVision;
    private CameraShakeManagerV2 camShakeManager;
    private GiveExpToPlayer giveExpToPlayer;
    [HideInInspector] public Transform handCastingPoint;
    private MaterialFlashWhenHit flashWhenHit;
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
        if (isRanged) RangeHitDetection(collision);
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
        if (isRanged) RangeHitDetection(collider);
        var grandparentTransform = collider.transform.parent != null ? collider.transform.parent.parent : null;
        if (grandparentTransform != null && grandparentTransform.TryGetComponent(out AiReferences refs))
        {
            HitRegistered(refs, weakPointHit);
        }
        else
        {
            PlayExplosionSounds();
            //if (!weakPointHit)
            //{
            //    AttemptSpawnDecal(collider.transform.position, Vector3.up, collider.gameObject);
            //}
        }
        SpawnExplosion();
        AfterExplosion();
    }

    private void HitRegistered(AiReferences refs, bool weakPointHit)
    {
        _collider.enabled = false;
        aiHealth = refs.Health;
        aiVision = refs.Vision;
        giveExpToPlayer = refs.GiveExp;
        flashWhenHit = refs.MaterialFlash;
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
        flashWhenHit.StopAllCoroutines();
        if (weakPointHit)
        {
            Headshot(refs);
            giveExpToPlayer.wasLastShotAHeadshot = true;
            flashWhenHit.FlashHeadshot();
            return;
        }
        giveExpToPlayer.wasLastShotAHeadshot = false;
        flashWhenHit.Flash();
        aiHealth.TakeDamage(damage, false, wasLastHitCritical);
        ApplySpecialEffect(refs);
        HitGetsYouNoticed();
    }

    private void RangeHitDetection(Collision collision)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(collision.GetContact(0).point, rangeRadius, overlapResults, detectionLayer);
        HashSet<AiReferences> alreadyHit = new HashSet<AiReferences>();
        for (int i = 0; i < hitCount; i++)
        {
            var col = overlapResults[i];
            if (col == collision.collider) continue;
            if (col.transform.TryGetComponent(out AiReferences areaRefs))
            {
                if (areaRefs.setOnFire != null && !alreadyHit.Contains(areaRefs))
                {
                    alreadyHit.Add(areaRefs);
                    HitRegistered(areaRefs, false);
                }
            }
        }
    }

    private void RangeHitDetection(Collider collider)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(collider.transform.position, rangeRadius, overlapResults, detectionLayer);
        HashSet<AiReferences> alreadyHit = new HashSet<AiReferences>();
        for (int i = 0; i < hitCount; i++)
        {
            var col = overlapResults[i];
            if (col == collider) continue;
            if (col.transform.TryGetComponent(out AiReferences areaRefs))
            {
                if (areaRefs.setOnFire != null && !alreadyHit.Contains(areaRefs))
                {
                    alreadyHit.Add(areaRefs);
                    HitRegistered(areaRefs, false);
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
        if (aiHealth.hp > 0 && doesThisSpellSetOnFire && aiRefs.setOnFire) aiRefs.setOnFire.enabled = true;
    }

    private void RandomizeCritical(AiReferences refs)
    {
        int criticalThreshold = (ammo.ammo < ammo.maxAmmo * 0.2f) ? 5 : 9;
        if (Random.Range(0, 10) >= criticalThreshold || iWillBeCritical)
        {
            if (refs.damageTakenCritical) refs.damageTakenCritical.Play();
            damage = (int)(originalDamage * 1.5f);
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

    private void HitGetsYouNoticed()
    {
        if (aiVision)
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
                        playerMove.AddExplosionJump(rocketJumpForce * 2, explosionCenter, rangeRadius);
                        var distance = Vector3.Distance(playerMove.transform.position, explosionCenter);
                        var proximityFactor = 1f - distance / rangeRadius;
                        proximityFactor = Mathf.Clamp01(proximityFactor);
                        PlayerHP.instance.TakeDamage(Mathf.RoundToInt(rocketJumpForce * proximityFactor));

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

    public void ReturnToPool()
    {
        StopCoroutine(autoKill);
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
        //Debug.Log($"Hit object: {hitObject.name}, Layer: {hitObject.layer}, Tag: {hitObject.tag}");

    }

    /*
    private void AttemptSpawnDecal(Vector3 point, Vector3 normal, GameObject objectHit) //dynamic objects
    {
        if (!objectHit || !objectHit.isStatic) return;

        if (SpellDecalManager.Instance != null)
        {
            string hitTag = objectHit.tag;
            SpellDecalManager.Instance.SpawnDecal(point + normal * 0.01f,Quaternion.LookRotation(normal),spellID,hitTag.GetHashCode());
        }
    }
    */

    private void ResetBulletState()
    {
        _explosionFxGameObject.SetActive(false);
        _collider.enabled = true;
        _renderer.enabled = true;
        _rigidbody.isKinematic = false;
        _light.enabled = true;
        hitSomething = false;
    }
}