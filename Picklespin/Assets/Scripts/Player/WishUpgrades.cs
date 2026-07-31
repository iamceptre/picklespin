using System;
using System.Collections.Generic;
using UnityEngine;

// Wish buffs that would otherwise have to be written into a prefab field, which the
// Editor would keep between sessions - consumers read them at the point of use.
// Static, so it survives a scene reload: AngelWishMenu.Awake calls ResetAll.
public static class WishUpgrades
{
    public static float CastDurationMultiplier { get; private set; } = 1f;
    public static float MagickaCostMultiplier { get; private set; } = 1f;
    public static float CooldownMultiplier { get; private set; } = 1f;
    public static float RecoilScale { get; private set; } = 1f;
    public static float ExpGatherMultiplier { get; private set; } = 1f;
    public static float EnemySpeedMultiplier { get; private set; } = 1f;
    public static float RocketJumpForceMultiplier { get; private set; } = 1f;
    public static bool RocketJumpSelfDamage { get; private set; } = true;

    // keyed by Bullet.spellName ("Netherlight", "Fireball", "light")
    private static readonly Dictionary<string, float> spellDamage = new(StringComparer.OrdinalIgnoreCase);

    public static float SpellDamageMultiplier(string spellName)
    {
        if (string.IsNullOrEmpty(spellName)) return 1f;
        return spellDamage.TryGetValue(spellName, out float multiplier) ? multiplier : 1f;
    }

    public static void MultiplySpellDamage(string spellName, float factor)
    {
        if (string.IsNullOrEmpty(spellName)) return;
        spellDamage[spellName] = SpellDamageMultiplier(spellName) * factor;
    }

    public static void MultiplyCastDuration(float factor) => CastDurationMultiplier *= factor;
    public static void MultiplyMagickaCost(float factor) => MagickaCostMultiplier *= factor;
    public static void MultiplyCooldown(float factor) => CooldownMultiplier *= factor;
    public static void MultiplyRecoil(float factor) => RecoilScale *= factor;
    public static void MultiplyExpGather(float factor) => ExpGatherMultiplier *= factor;
    public static void MultiplyEnemySpeed(float factor) => EnemySpeedMultiplier *= factor;
    public static void MultiplyRocketJumpForce(float factor) => RocketJumpForceMultiplier *= factor;
    public static void DisableRocketJumpSelfDamage() => RocketJumpSelfDamage = false;

    public static void ResetAll()
    {
        CastDurationMultiplier = 1f;
        MagickaCostMultiplier = 1f;
        CooldownMultiplier = 1f;
        RecoilScale = 1f;
        ExpGatherMultiplier = 1f;
        EnemySpeedMultiplier = 1f;
        RocketJumpForceMultiplier = 1f;
        RocketJumpSelfDamage = true;
        spellDamage.Clear();
    }
}
