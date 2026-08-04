public static class VesperUpgrades
{
    private const int SiphonedMagicka = 3;

    private static bool siphons;

    public static readonly ClassUpgrade[] Levels =
    {
        new()
        {
            Name = "<b>Deeper Vessel</b>",
            Effect = "Max magicka +25%, and magicka is your life",
            Apply = () => { if (Ammo.instance) Ammo.instance.MultiplyMaxMana(1.25f); }
        },
        new()
        {
            Name = "<b>Frugal Rites</b>",
            Effect = "Magicka cost -25%",
            Apply = () => WishUpgrades.MultiplyMagickaCost(0.75f)
        },
        new()
        {
            Name = "<b>Siphon</b>",
            Effect = $"Every spell that lands returns {SiphonedMagicka} magicka",
            Apply = () => siphons = true
        }
    };

    public static void OnSpellHit()
    {
        if (!siphons || !Ammo.instance) return;
        Ammo.instance.GiveManaToPlayer(SiphonedMagicka, true);
    }

    public static void ResetAll() => siphons = false;
}
