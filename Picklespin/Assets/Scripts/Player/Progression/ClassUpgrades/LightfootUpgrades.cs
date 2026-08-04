public static class LightfootUpgrades
{
    private const int CuttingWindDamage = 25;

    public static int DashDamage { get; private set; }

    public static readonly ClassUpgrade[] Levels =
    {
        new()
        {
            Name = "<b>Fleet</b>",
            Effect = "Max speed +15%, jump power +10%",
            Apply = () =>
            {
                if (!PlayerMovement.Instance) return;
                PlayerMovement.Instance.MultiplyMaxSpeed(1.15f);
                PlayerMovement.Instance.MultiplyJumpPower(1.1f);
            }
        },
        new()
        {
            Name = "<b>Momentum</b>",
            Effect = "Speed damage +30%",
            Apply = () => { if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplySpeedDamage(1.3f); }
        },
        new()
        {
            Name = "<b>Cutting Wind</b>",
            Effect = $"The dash tears what it stuns for {CuttingWindDamage}",
            Apply = () => DashDamage = CuttingWindDamage
        }
    };

    public static void ResetAll() => DashDamage = 0;
}
