public static class BlastfoolUpgrades
{
    private const float DefaultGroundedDamage = 0.2f;
    private const float DefaultAirborneDamage = 0.7f;
    private const float SkyborneGroundedDamage = 0.45f;
    private const float SkyborneAirborneDamage = 1.1f;

    public static float GroundedDamage { get; private set; } = DefaultGroundedDamage;
    public static float AirborneDamage { get; private set; } = DefaultAirborneDamage;
    public const float RocketJumpDamage = 2f;

    public static readonly ClassUpgrade[] Levels =
    {
        new()
        {
            Name = "<b>Padded Bones</b>",
            Effect = "Rocket-jump self-damage -50%",
            Apply = () => WishUpgrades.MultiplyRocketJumpSelfDamage(0.5f)
        },
        new()
        {
            Name = "<b>Skyborne</b>",
            Effect = "Airborne damage x1.1 instead of x0.7, grounded x0.45 instead of x0.2",
            Apply = () =>
            {
                GroundedDamage = SkyborneGroundedDamage;
                AirborneDamage = SkyborneAirborneDamage;
            }
        },
        new()
        {
            Name = "<b>Powder Keg</b>",
            Effect = "Rocket-jump force +50% and the blast no longer bites you",
            Apply = () =>
            {
                WishUpgrades.MultiplyRocketJumpForce(1.5f);
                WishUpgrades.DisableRocketJumpSelfDamage();
            }
        }
    };

    public static void ResetAll()
    {
        GroundedDamage = DefaultGroundedDamage;
        AirborneDamage = DefaultAirborneDamage;
    }
}
