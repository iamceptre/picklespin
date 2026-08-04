public static class WandererUpgrades
{
    private const int ScavengedMagicka = 15;

    private static bool scavenges;

    public static readonly ClassUpgrade[] Levels =
    {
        new()
        {
            Name = "<b>Callused Hands</b>",
            Effect = "Max health +20%, max stamina +20%",
            Apply = () =>
            {
                if (PlayerHP.Instance) PlayerHP.Instance.MultiplyMaxHp(1.2f);
                if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyMaxStamina(1.2f);
            }
        },
        new()
        {
            Name = "<b>Steady Aim</b>",
            Effect = "Critical chance +15%, spell recoil -15%",
            Apply = () =>
            {
                WishUpgrades.AddCriticalChance(0.15f);
                WishUpgrades.MultiplyRecoil(0.85f);
            }
        },
        new()
        {
            Name = "<b>Scavenger</b>",
            Effect = $"Every kill returns {ScavengedMagicka} magicka",
            Apply = () => scavenges = true
        }
    };

    public static void OnEnemyKilled()
    {
        if (!scavenges || !Ammo.instance) return;
        Ammo.instance.GiveManaToPlayer(ScavengedMagicka, true);
    }

    public static void ResetAll() => scavenges = false;
}
