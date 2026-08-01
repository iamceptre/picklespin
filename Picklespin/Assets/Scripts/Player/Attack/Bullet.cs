using UnityEngine;
using FMODUnity;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    [Header("Spell")]
    [SerializeField] private SpellId spell = SpellId.Netherlight;
    public int magickaCost = 30;
    public int speed = 60;
    public float myCooldown;
    [SerializeField, Tooltip("seconds in the air before the shot gives up and returns to the pool")]
    private float timeBeforeOff = 2f;
    [SerializeField, Tooltip("seconds after landing before the shot returns to the pool on its own - the explosion animation normally takes it back long before this, so it is the safety net that keeps a spell from being parked in the level with its light on")]
    private float timeAfterImpact = 5f;

    [Header("Sound")]
    [SerializeField] private EventReference shootSound;
    public EventReference pullupSound;

    public LightSpell lightSpell;

    public SpellId Spell => spell;

    public string DisplayName => spell.ToString();

    public bool IsCharged => casting && casting.IsCharged;

    [HideInInspector] public bool hitSomething;

    private SpellDamage spellDamage;
    private SpellIgnite ignite;
    private SpellAreaOfEffect areaOfEffect;
    private SpellRocketJump rocketJump;
    private SpellImpactFx impactFx;
    private SpellDecal decal;
    private SpellCameraShake cameraShake;
    private SpellCasting casting;
    private ApplyProjectileForce applyProjectileForce;

    private ObjectPool<Bullet> _pool;
    private IEnumerator autoKill;
    private WaitForSeconds autoKillTime;
    private WaitForSeconds afterImpactTime;

    private Renderer _renderer;
    private Rigidbody _rigidbody;
    private SphereCollider _collider;

    private bool released;
    private bool impactSoundClaimed;

    private readonly HashSet<AiReferences> piercedThisFlight = new();
    private Vector3 pierceVelocity;
    private bool convertedThisFlight;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        autoKillTime = new WaitForSeconds(timeBeforeOff);
        afterImpactTime = new WaitForSeconds(timeAfterImpact);

        spellDamage = GetComponent<SpellDamage>();
        ignite = GetComponent<SpellIgnite>();
        areaOfEffect = GetComponent<SpellAreaOfEffect>();
        rocketJump = GetComponent<SpellRocketJump>();
        impactFx = GetComponent<SpellImpactFx>();
        decal = GetComponent<SpellDecal>();
        cameraShake = GetComponent<SpellCameraShake>();
        casting = GetComponent<SpellCasting>();
        applyProjectileForce = GetComponent<ApplyProjectileForce>();
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

    private void ArmImpactTimer()
    {
        if (autoKill != null) StopCoroutine(autoKill);
        autoKill = ReturnAfterImpact();
        StartCoroutine(autoKill);
    }

    private IEnumerator ReturnAfterImpact()
    {
        yield return afterImpactTime;
        ReturnToPool();
    }

    public void OnShoot()
    {
        if (impactFx) impactFx.OnShoot();
        if (spellDamage) spellDamage.OnShoot();

        RuntimeManager.PlayOneShot(shootSound, transform.position);
        if (applyProjectileForce) applyProjectileForce.Set();
        pierceVelocity = _rigidbody.linearVelocity;
    }

    public bool TryClaimImpactSound()
    {
        if (impactSoundClaimed) return false;
        impactSoundClaimed = true;
        return true;
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
        ArmImpactTimer();
        Vector3 impactPoint = collision.GetContact(0).point;
        collision.collider.TryGetComponent(out AiReferences refs);
        if (areaOfEffect) areaOfEffect.Burst(refs, impactPoint);

        if (refs)
        {
            ApplyHit(refs, false);
        }
        else
        {
            if (impactFx) impactFx.PlaySounds();
            if (decal) decal.TrySpawn(collision);
        }

        Explode(impactPoint);
        CommandAlliesIfLightSpell(impactPoint);
        if (rocketJump) rocketJump.Apply(impactPoint);
        AfterExplosion();
    }

    private void GeneralAfterHit(Collider collider, bool weakPointHit)
    {
        ArmImpactTimer();
        Vector3 impactPoint = collider.transform.position;

        AiReferences refs = ResolveHitboxOwner(collider);
        if (areaOfEffect) areaOfEffect.Burst(refs, impactPoint);

        if (refs) ApplyHit(refs, weakPointHit);
        else if (impactFx) impactFx.PlaySounds();

        Explode(impactPoint);
        CommandAlliesIfLightSpell(impactPoint);
        AfterExplosion();
    }

    private static AiReferences ResolveHitboxOwner(Collider collider)
    {
        Transform grandparent = collider.transform.parent != null ? collider.transform.parent.parent : null;
        return grandparent != null && grandparent.TryGetComponent(out AiReferences refs) ? refs : null;
    }

    private void Pierce(AiReferences refs, bool weakPointHit)
    {
        if (refs != null && piercedThisFlight.Add(refs) && refs.Health && refs.Health.IsAlive)
        {
            ApplyHit(refs, weakPointHit, keepFlying: true);
        }

        if (pierceVelocity.sqrMagnitude > 0f) _rigidbody.linearVelocity = pierceVelocity;
    }

    public void ApplyHit(AiReferences refs, bool weakPointHit, bool keepFlying = false)
    {
        if (!keepFlying) _collider.enabled = false;
        if (!refs.Health) return;

        if (lightSpell && !keepFlying) lightSpell.FadeOut();

        if (lightSpell && PlayerClasses.LightSpellConverts)
        {
            convertedThisFlight = true;
            ConvertedAlly.Convert(refs);
            return;
        }

        if (spellDamage) spellDamage.Apply(refs, weakPointHit);
        if (ignite) ignite.TryIgnite(refs);
    }

    private void Explode(Vector3 impactPoint)
    {
        if (impactFx) impactFx.PlayImpact();
        if (cameraShake) cameraShake.PlayImpact(impactPoint);
    }

    private void CommandAlliesIfLightSpell(Vector3 point)
    {
        if (lightSpell && PlayerClasses.LightSpellConverts && !convertedThisFlight)
        {
            ConvertedAlly.CommandAll(point);
        }
    }

    public void AfterExplosion()
    {
        _collider.enabled = false;
        _renderer.enabled = false;
        _rigidbody.isKinematic = true;
        if (impactFx) impactFx.Shutdown();
    }

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

    private void ResetBulletState()
    {
        _collider.enabled = true;
        _renderer.enabled = true;
        _rigidbody.isKinematic = false;
        hitSomething = false;
        released = false;
        impactSoundClaimed = false;
        piercedThisFlight.Clear();
        pierceVelocity = Vector3.zero;
        convertedThisFlight = false;
        if (impactFx) impactFx.ResetState();
        if (spellDamage) spellDamage.ResetState();
    }
}
