using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class SpellDamage : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    [SerializeField, Tooltip("scale damage by player speed (×0.25 standing – ×2 at bhop speeds); off = flat damage")]
    private bool useSpeedDamageMultiplier = true;

    [HideInInspector] public bool iWillBeCritical;

    private Bullet bullet;
    private Ammo ammo;
    private int originalDamage;
    private bool wasLastHitCritical;

    private float flightDamageMultiplier = 1f;

    private void Awake()
    {
        bullet = GetComponent<Bullet>();
        originalDamage = damage;
    }

    private void Start() => ammo = Ammo.instance;

    public void OnShoot() => flightDamageMultiplier = PlayerClasses.FlightDamageMultiplier;

    public void ResetState()
    {
        flightDamageMultiplier = 1f;
        SetCriticalToNo();
    }

    public void Apply(AiReferences refs, bool weakPointHit)
    {
        PlayHitSound(refs);

        if (refs.GiveExp) refs.GiveExp.wasLastShotAHeadshot = weakPointHit;

        if (refs.MaterialFlash)
        {
            if (weakPointHit) refs.MaterialFlash.FlashHeadshot();
            else refs.MaterialFlash.Flash();
        }

        if (refs.Vision) refs.Vision.HitShowsMePlayer();

        if (weakPointHit) Headshot(refs);
        else refs.Health.TakeDamage(ScaledDamage(), false, wasLastHitCritical);
    }

    private void PlayHitSound(AiReferences refs)
    {
        if (bullet && bullet.IsCharged)
        {
            if (refs.damageTakenBig && bullet.TryClaimImpactSound())
            {
                SetCriticalToNo();
                refs.damageTakenBig.Play();
            }
            return;
        }

        if (refs.damageTakenSmall)
        {
            RandomizeCritical(refs);
            refs.damageTakenSmall.Play();
        }
    }

    private void Headshot(AiReferences refs)
    {
        refs.Health.TakeDamage(ScaledDamage(), true, wasLastHitCritical);
        if (refs.HeadshotParticle) refs.HeadshotParticle.Play();
        if (refs.damageTakenEyeshot) refs.damageTakenEyeshot.Play();
    }

    private int ScaledDamage()
    {
        float multiplier = WishUpgrades.SpellDamageMultiplier(bullet.Spell)
                           * PlayerClasses.ProjectileDamageMultiplier
                           * flightDamageMultiplier;
        if (useSpeedDamageMultiplier && PlayerClasses.SpeedDamageActive && PlayerMovement.Instance)
        {
            multiplier *= PlayerMovement.Instance.SpeedDamageMultiplier;
        }
        return Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }

    private void RandomizeCritical(AiReferences refs)
    {
        float criticalChance = (ammo && ammo.IsLow ? 0.5f : 0.1f) + WishUpgrades.CriticalChanceBonus;
        if (Random.value < criticalChance || iWillBeCritical)
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
}
