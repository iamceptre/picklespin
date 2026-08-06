using UnityEngine;

// Read every frame by the round timer, so the pref is fetched once and kept.
public static class HardMode
{
    private static bool loaded;
    private static bool enabled;

    public static bool Enabled
    {
        get
        {
            if (!loaded)
            {
                enabled = PlayerPrefs.GetInt(SettingsDefaults.HardModeKey, SettingsDefaults.HardMode) != 0;
                loaded = true;
            }
            return enabled;
        }
    }

    public static void Set(bool on)
    {
        enabled = on;
        loaded = true;
        PlayerPrefs.SetInt(SettingsDefaults.HardModeKey, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticsOnPlay()
    {
        loaded = false;
        enabled = false;
    }
}
