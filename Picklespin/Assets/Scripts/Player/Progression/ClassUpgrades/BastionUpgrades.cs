public static class BastionUpgrades
{
    private const float DefaultCooldownMultiplier = 1.3f;
    private const int MendedHealth = 15;

    public static float SpellCooldownMultiplier { get; private set; } = DefaultCooldownMultiplier;

    private static bool mends;

    public static readonly ClassUpgrade[] Levels =
    {
        new()
        {
            Name = "<b>Bulwark</b>",
            Effect = "Max health +30%",
            Apply = () => { if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(1.3f); }
        },
        new()
        {
            Name = "<b>Unyielding</b>",
            Effect = "The slow hand and the slow feet are yours no more",
            Apply = () =>
            {
                SpellCooldownMultiplier = 1f;
                if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyMaxSpeed(4f / 3f);
            }
        },
        new()
        {
            Name = "<b>Sanctified Flesh</b>",
            Effect = $"Every kill mends {MendedHealth} health",
            Apply = () => mends = true
        }
    };

    public static void OnEnemyKilled()
    {
        if (!mends || !PlayerHP.Instance) return;
        PlayerHP.Instance.ModifyHP(MendedHealth);
    }

    public static void ResetAll()
    {
        SpellCooldownMultiplier = DefaultCooldownMultiplier;
        mends = false;
    }
}
