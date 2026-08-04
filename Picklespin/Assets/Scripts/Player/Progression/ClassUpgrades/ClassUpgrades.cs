using System;
using UnityEngine;

public static class ClassUpgrades
{
    public const int MaxLevel = 3;

    public const int ExpPerLevel = 1000;

    private const int HealsPerLevel = 3;

    public static int Level { get; private set; }

    public static event Action LevelChanged;

    private static PlayerClassId leveledAs = PlayerClassId.None;
    private static int angelsHealed;

    public static int NextLevel => Level + 1;

    public static ClassUpgrade Next
    {
        get
        {
            ClassUpgrade[] levels = LevelsFor(PlayerClasses.Chosen);
            return NextLevel <= levels.Length ? levels[NextLevel - 1] : null;
        }
    }

    public static int RequiredExp(int level) => level * ExpPerLevel;
    public static int CarriedExp => PlayerEXP.instance ? PlayerEXP.instance.playerExpAmount : 0;
    public static int MissingExp(int level) => Mathf.Max(0, RequiredExp(level) - CarriedExp);
    public static bool CanAfford(int level) => MissingExp(level) == 0;

    public static bool IsLevelDue => angelsHealed > 0 && angelsHealed % HealsPerLevel == 0 && Next != null;

    public static void CountAngelHealed() => angelsHealed++;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticsOnPlay()
    {
        LevelChanged = null;
        ResetAll();
    }

    public static bool TakeNext()
    {
        ClassUpgrade upgrade = Next;
        if (upgrade == null || !CanAfford(NextLevel)) return false;

        Grant(upgrade);
        return true;
    }

    public static void SetLevel(int level)
    {
        int target = Mathf.Clamp(level, 0, MaxLevel);
        while (Level < target)
        {
            ClassUpgrade upgrade = Next;
            if (upgrade == null) return;
            Grant(upgrade);
        }
    }

    public static void OnEnemyKilled()
    {
        switch (PlayerClasses.Chosen)
        {
            case PlayerClassId.None: WandererUpgrades.OnEnemyKilled(); break;
            case PlayerClassId.Umbral: UmbralUpgrades.OnEnemyKilled(); break;
            case PlayerClassId.Bastion: BastionUpgrades.OnEnemyKilled(); break;
        }
    }

    public static void OnSpellHit()
    {
        if (PlayerClasses.Chosen == PlayerClassId.Vesper) VesperUpgrades.OnSpellHit();
    }

    public static void ClassChanged()
    {
        if (leveledAs == PlayerClasses.Chosen) return;

        leveledAs = PlayerClasses.Chosen;
        if (Level > 0) Forget();
    }

    public static void ResetAll()
    {
        angelsHealed = 0;
        leveledAs = PlayerClasses.Chosen;
        Forget();
    }

    private static void Forget()
    {
        Level = 0;
        WandererUpgrades.ResetAll();
        VesperUpgrades.ResetAll();
        LightfootUpgrades.ResetAll();
        UmbralUpgrades.ResetAll();
        BlastfoolUpgrades.ResetAll();
        BastionUpgrades.ResetAll();
        SanctusUpgrades.ResetAll();
        LevelChanged?.Invoke();
    }

    private static void Grant(ClassUpgrade upgrade)
    {
        Level++;
        leveledAs = PlayerClasses.Chosen;
        upgrade.Apply?.Invoke();
        LevelChanged?.Invoke();
    }

    private static ClassUpgrade[] LevelsFor(PlayerClassId playerClass) => playerClass switch
    {
        PlayerClassId.Vesper => VesperUpgrades.Levels,
        PlayerClassId.Lightfoot => LightfootUpgrades.Levels,
        PlayerClassId.Umbral => UmbralUpgrades.Levels,
        PlayerClassId.Blastfool => BlastfoolUpgrades.Levels,
        PlayerClassId.Bastion => BastionUpgrades.Levels,
        PlayerClassId.Sanctus => SanctusUpgrades.Levels,
        _ => WandererUpgrades.Levels
    };
}
