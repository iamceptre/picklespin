using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class SpellDamage : MonoBehaviour
{
    [SerializeField] private int damage = 15;

    [HideInInspector] public bool iWillBeCritical;

    private Bullet bullet;
    private Ammo ammo;
    private float criticalMultiplier = 1f;
    private bool wasLastHitCritical;

    private float flightDamageMultiplier = 1f;

    private void Awake() => bullet = GetComponent<Bullet>();

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
        float multiplier = criticalMultiplier
                           * WishUpgrades.SpellDamageMultiplier(bullet.Spell)
                           * PlayerClasses.ProjectileDamageMultiplier
                           * flightDamageMultiplier;
        if (PlayerClasses.SpeedDamageActive && PlayerMovement.Instance)
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
            criticalMultiplier = PhiMath.PHI;
            wasLastHitCritical = true;
        }
        else
        {
            SetCriticalToNo();
        }
    }

    private void SetCriticalToNo()
    {
        criticalMultiplier = 1f;
        wasLastHitCritical = false;
    }
}
