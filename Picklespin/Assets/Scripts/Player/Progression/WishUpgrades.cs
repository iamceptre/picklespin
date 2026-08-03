using System.Collections.Generic;
using UnityEngine;

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
    public static float CriticalChanceBonus { get; private set; }

    private static readonly Dictionary<SpellId, float> spellDamage = new();

    public static float SpellDamageMultiplier(SpellId spell) =>
        spellDamage.TryGetValue(spell, out float multiplier) ? multiplier : 1f;

    public static void MultiplySpellDamage(SpellId spell, float factor) =>
        spellDamage[spell] = SpellDamageMultiplier(spell) * factor;

    public static void MultiplyCastDuration(float factor) => CastDurationMultiplier *= factor;
    public static void MultiplyMagickaCost(float factor) => MagickaCostMultiplier *= factor;
    public static void MultiplyCooldown(float factor) => CooldownMultiplier *= factor;
    public static void MultiplyRecoil(float factor) => RecoilScale *= factor;
    public static void MultiplyExpGather(float factor) => ExpGatherMultiplier *= factor;
    public static void MultiplyEnemySpeed(float factor) => EnemySpeedMultiplier *= factor;
    public static void MultiplyRocketJumpForce(float factor) => RocketJumpForceMultiplier *= factor;
    public static void DisableRocketJumpSelfDamage() => RocketJumpSelfDamage = false;
    public static void AddCriticalChance(float amount) => CriticalChanceBonus += amount;

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
        CriticalChanceBonus = 0f;
        spellDamage.Clear();
    }
}
