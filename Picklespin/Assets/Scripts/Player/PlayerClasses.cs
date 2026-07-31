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

    // sprinting and jumping come out of the magicka pool (Ammo.SpendAsStamina)
    public static bool StaminaSharesMagicka => Chosen == PlayerClassId.Umbral;

    // Umbral carries one spell and no inventory; -1 = the player picks freely
    public static int LockedSpellIndex { get; private set; } = -1;

    public const float ChargedBarThreshold = 0.5f;
    public static bool ChargedBarReady =>
        Ammo.instance && Ammo.instance.maxAmmo > 0 &&
        (float)Ammo.instance.ammo / Ammo.instance.maxAmmo > ChargedBarThreshold;

    public static float RocketJumpForceMultiplier => Chosen == PlayerClassId.Blastfool ? 3.5f : 1f;
    public static float RocketJumpSelfDamageMultiplier => Chosen == PlayerClassId.Blastfool ? 2f : 1f;

    public static float RocketJumpMinProximity => Chosen == PlayerClassId.Blastfool ? 0.75f : 0f;

    public const float BlastfoolGroundedDamage = 0.2f;
    public const float BlastfoolAirborneDamage = 0.7f;
    public const float BlastfoolRocketJumpDamage = 2f;


    public static float FlightDamageMultiplier
    {
        get
        {
            if (Chosen != PlayerClassId.Blastfool) return 1f;
            var movement = PlayerMovement.Instance;
            if (!movement) return BlastfoolGroundedDamage;
            if (movement.IsRocketJumping) return BlastfoolRocketJumpDamage;
            return movement.IsGroundedStable ? BlastfoolGroundedDamage : BlastfoolAirborneDamage;
        }
    }

    public static float RecoilScale => Chosen == PlayerClassId.Blastfool ? 0.1f : 1f;

    public static bool PiercingProjectiles => Chosen == PlayerClassId.Bastion;
    public static float SpellCooldownMultiplier => Chosen == PlayerClassId.Bastion ? 1.5f : 1f;

    public static bool LightSpellConverts => Chosen == PlayerClassId.Sanctus;

    public static float ProjectileDamageMultiplier => Chosen switch
    {
        PlayerClassId.Bastion => 2f,
        PlayerClassId.Sanctus => 0.25f,
        _ => 1f
    };

    public static void Choose(PlayerClassId id, int lockedSpellIndex = -1)
    {
        Chosen = id;
        LockedSpellIndex = lockedSpellIndex;
        WasOffered = true;
        ApplyStatChanges(id);
        Changed?.Invoke();
    }


    private static void ApplyStatChanges(PlayerClassId id)
    {
        switch (id)
        {
            case PlayerClassId.Vesper:
                if (Ammo.instance) Ammo.instance.MultiplyMaxMana(2f);
                break;

            case PlayerClassId.Lightfoot:
                if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(0.5f);
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
                if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(0.5f);
                break;

            case PlayerClassId.Bastion:
                if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(2f);
                if (PlayerMovement.Instance)
                {
                    PlayerMovement.Instance.MultiplyMaxSpeed(0.8f);
                    PlayerMovement.Instance.MultiplyJumpPower(0.8f);
                }
                break;
        }
    }

    public static void Skip()
    {
        Chosen = PlayerClassId.None;
        LockedSpellIndex = -1;
        WasOffered = true;
        Changed?.Invoke();
    }

    public static void ResetAll()
    {
        Chosen = PlayerClassId.None;
        LockedSpellIndex = -1;
        WasOffered = false;
        Changed?.Invoke();
    }
}
