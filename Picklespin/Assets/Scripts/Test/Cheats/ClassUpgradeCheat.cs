using UnityEngine;

public class ClassUpgradeCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static ClassUpgradeCheat host;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (host) return;

        GameObject go = new(nameof(ClassUpgradeCheat)) { hideFlags = HideFlags.HideInHierarchy };
        DontDestroyOnLoad(go);
        host = go.AddComponent<ClassUpgradeCheat>();

        DevLog.Info($"{nameof(ClassUpgradeCheat)} armed: Space + 1/2/3 takes that class-upgrade level, " +
                    $"Space + L opens the upgrade menu, Space + P grants {ClassUpgrades.ExpPerLevel} EXP", host);
    }

    private void Update()
    {
        if (host != this || !InputCompat.GetKey(KeyCode.Space)) return;

        for (int level = 1; level <= ClassUpgrades.MaxLevel; level++)
        {
            if (!InputCompat.GetKeyDown(KeyCode.Alpha1 + level - 1)) continue;

            TakeLevel(level);
            return;
        }

        if (InputCompat.GetKeyDown(KeyCode.L)) OpenMenu();
        else if (InputCompat.GetKeyDown(KeyCode.P)) GrantExp();
    }

    private void GrantExp()
    {
        if (!PlayerEXP.instance)
        {
            DevLog.Info($"{nameof(ClassUpgradeCheat)}: no {nameof(PlayerEXP)} in the scene to give EXP to.", this);
            return;
        }

        PlayerEXP.instance.GivePlayerExp(ClassUpgrades.ExpPerLevel, "Cheat");
        Feedback($"+{ClassUpgrades.ExpPerLevel} exp");
    }

    private void TakeLevel(int level)
    {
        if (level <= ClassUpgrades.Level)
        {
            DevLog.Info($"{nameof(ClassUpgradeCheat)}: already at class level {ClassUpgrades.Level} - levels only go up.", this);
            return;
        }

        ClassUpgrades.SetLevel(level);
        Feedback($"class level {ClassUpgrades.Level}");
    }

    private void OpenMenu()
    {
        if (ClassUpgradeMenu.Instance && ClassUpgradeMenu.Instance.AskForUpgrade(false))
        {
            Feedback("class upgrade menu");
            return;
        }

        DevLog.Info($"{nameof(ClassUpgradeCheat)}: nothing left to offer - class level is {ClassUpgrades.Level} of {ClassUpgrades.MaxLevel}.", this);
    }

    private static void Feedback(string what)
    {
        if (CheatActivatedFeedback.instance) CheatActivatedFeedback.instance.Do(what);
    }
#endif
}
