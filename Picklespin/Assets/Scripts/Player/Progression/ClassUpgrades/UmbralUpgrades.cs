public static class UmbralUpgrades
{
    private const float DefaultChargedBarThreshold = 0.5f;
    private const float LoweredChargedBarThreshold = 0.3f;
    private const int FedMagicka = 25;

    public static float ChargedBarThreshold { get; private set; } = DefaultChargedBarThreshold;

    private static bool feeds;

    public static readonly ClassUpgrade[] Levels =
    {
        new()
        {
            Name = "<b>Bottomless</b>",
            Effect = "The dark runs 20% deeper and tires you 25% slower",
            Apply = () =>
            {
                if (Ammo.instance) Ammo.instance.MultiplyMaxMana(1.2f);
                if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyFatigability(0.75f);
            }
        },
        new()
        {
            Name = "<b>Low Tide</b>",
            Effect = "Your shell still bursts with the bar barely a third full",
            Apply = () => ChargedBarThreshold = LoweredChargedBarThreshold
        },
        new()
        {
            Name = "<b>The Dark Feeds</b>",
            Effect = $"Every kill returns {FedMagicka} to the bar",
            Apply = () => feeds = true
        }
    };

    public static void OnEnemyKilled()
    {
        if (!feeds || !Ammo.instance) return;
        Ammo.instance.GiveManaToPlayer(FedMagicka, true);
    }

    public static void ResetAll()
    {
        ChargedBarThreshold = DefaultChargedBarThreshold;
        feeds = false;
    }
}
