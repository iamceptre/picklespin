public static class SanctusUpgrades
{
    private const float DefaultOwnDamageMultiplier = 0.25f;
    private const float BlessedOwnDamageMultiplier = 0.5f;
    private const int DefaultMaxAllies = 1;
    private const int CongregationMaxAllies = 3;

    public static float OwnDamageMultiplier { get; private set; } = DefaultOwnDamageMultiplier;
    public static float AllyStrikeMultiplier { get; private set; } = 1f;
    public static int MaxAllies { get; private set; } = DefaultMaxAllies;

    public static readonly ClassUpgrade[] Levels =
    {
        new()
        {
            Name = "<b>Blessed Hand</b>",
            Effect = "Your own spells sting twice as hard",
            Apply = () => OwnDamageMultiplier = BlessedOwnDamageMultiplier
        },
        new()
        {
            Name = "<b>Zealots</b>",
            Effect = "Those you turn strike twice as hard",
            Apply = () => AllyStrikeMultiplier = 2f
        },
        new()
        {
            Name = "<b>Congregation</b>",
            Effect = $"Keep {CongregationMaxAllies} of them at your side at once",
            Apply = () => MaxAllies = CongregationMaxAllies
        }
    };

    public static void ResetAll()
    {
        OwnDamageMultiplier = DefaultOwnDamageMultiplier;
        AllyStrikeMultiplier = 1f;
        MaxAllies = DefaultMaxAllies;
    }
}
