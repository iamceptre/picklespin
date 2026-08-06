using System;

public enum PlayerClassId
{
    None = 0,
    Vesper,
    Lightfoot,
    Umbral,
    Blastfool,
    Bastion,
    Sanctus
}
public static class PlayerClasses
{
    public static PlayerClassId Chosen { get; private set; } = PlayerClassId.None;

    public static bool WasOffered { get; private set; }

    public static event Action Changed;

    public static bool SpeedDamageActive => Chosen == PlayerClassId.Lightfoot;

    public static float AngelHealCostMultiplier => Chosen == PlayerClassId.Lightfoot ? 2f : 1f;

    public static bool MagickaIsHealth => Chosen is PlayerClassId.Vesper or PlayerClassId.Umbral;

    public static bool StaminaSharesMagicka => Chosen == PlayerClassId.Umbral;

    public static SpellId? LockedSpell { get; private set; }

    public static bool ChargedBarReady =>
        Ammo.instance && Ammo.instance.maxAmmo > 0 &&
        (float)Ammo.instance.ammo / Ammo.instance.maxAmmo > UmbralUpgrades.ChargedBarThreshold;

    public static float RocketJumpForceMultiplier => Chosen == PlayerClassId.Blastfool ? 2.5f : 1f;
    public static float RocketJumpSelfDamageMultiplier => Chosen == PlayerClassId.Blastfool ? 2.2f : 1f;

    public static float FlightDamageMultiplier
    {
        get
        {
            if (Chosen != PlayerClassId.Blastfool) return 1f;
            var movement = PlayerMovement.Instance;
            if (!movement) return BlastfoolUpgrades.GroundedDamage;
            if (movement.IsRocketJumping) return BlastfoolUpgrades.RocketJumpDamage;
            return movement.IsGroundedStable ? BlastfoolUpgrades.GroundedDamage : BlastfoolUpgrades.AirborneDamage;
        }
    }

    public static bool PiercingProjectiles => Chosen == PlayerClassId.Bastion;
    public static float SpellCooldownMultiplier =>
        Chosen == PlayerClassId.Bastion ? BastionUpgrades.SpellCooldownMultiplier : 1f;

    public static bool LightSpellConverts => Chosen == PlayerClassId.Sanctus;

    public static float ProjectileDamageMultiplier => Chosen switch
    {
        PlayerClassId.Bastion => 1.6f,
        PlayerClassId.Sanctus => SanctusUpgrades.OwnDamageMultiplier,
        _ => 1f
    };

    public static float RecoilScale => Chosen switch
    {
        PlayerClassId.Blastfool => 0.2f,
        PlayerClassId.Lightfoot => 0.5f,
        _ => 1f
    };

    public static void Choose(PlayerClassId id, SpellId? lockedSpell = null)
    {
        Chosen = id;
        LockedSpell = lockedSpell;
        WasOffered = true;
        ApplyClassEffects(id);
        Changed?.Invoke();
    }

    private static void ApplyClassEffects(PlayerClassId id)
    {
        switch (id)
        {
            case PlayerClassId.Vesper:
                if (Ammo.instance) Ammo.instance.MultiplyMaxMana(2f);
                break;

            case PlayerClassId.Lightfoot:
                if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(0.5f);
                if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyMaxSpeed(1.1f);
                Dash dash = Dash.Instance;
                if (dash)
                {
                    dash.MultiplyDashPower(3f);
                    dash.MultiplyDashRadius(3f);
                }
                break;

            case PlayerClassId.Umbral:
                if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyFatigability(0.5f);
                break;

            case PlayerClassId.Blastfool:
                if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(0.4f);
                break;

            case PlayerClassId.Bastion:
                if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(1.8f);
                if (PlayerMovement.Instance)
                {
                    PlayerMovement.Instance.MultiplyMaxSpeed(0.75f);
                    PlayerMovement.Instance.MultiplyJumpPower(0.75f);
                }
                break;

            case PlayerClassId.Sanctus:
                DropLightPickup();
                break;
        }
    }

    private static void DropLightPickup()
    {
        if (UnlockedSpells.instance && UnlockedSpells.instance.IsUnlocked(SpellId.Light)) return;
        if (SpellSpawner.instance) SpellSpawner.instance.SpawnSpell(SpellId.Light);
    }

    public static void Skip()
    {
        Chosen = PlayerClassId.None;
        LockedSpell = null;
        WasOffered = true;
        Changed?.Invoke();
    }

    public static void ResetAll()
    {
        Chosen = PlayerClassId.None;
        LockedSpell = null;
        WasOffered = false;
        Changed?.Invoke();
    }
}
