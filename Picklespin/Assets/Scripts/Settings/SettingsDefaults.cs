using UnityEngine;

public static class SettingsDefaults
{
    public const string VolumeKey = "Volume";
    public const string FramerateLimitKey = "FramerateLimit";
    public const string MouseSensitivityKey = "MouseSensitivity";
    public const string BaseFovKey = "BaseFOV";
    public const string CameraMotionKey = "CameraBobStrenght";
    public const string ScreenShakeKey = "ScreenShakeStrenght";
    public const string HardModeKey = "HardMode";

    public const float Volume = 100f;
    public const float FramerateLimit = 100f;
    public const float MouseSensitivity = 100f;
    public const float BaseFov = 80f;
    public const float CameraMotion = 100f;
    public const float ScreenShake = 100f;
    public const int HardMode = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void WriteMissing()
    {
        bool wroteAnything = false;
        wroteAnything |= WriteIfMissing(VolumeKey, Volume);
        wroteAnything |= WriteIfMissing(FramerateLimitKey, FramerateLimit);
        wroteAnything |= WriteIfMissing(MouseSensitivityKey, MouseSensitivity);
        wroteAnything |= WriteIfMissing(BaseFovKey, BaseFov);
        wroteAnything |= WriteIfMissing(CameraMotionKey, CameraMotion);
        wroteAnything |= WriteIfMissing(ScreenShakeKey, ScreenShake);
        wroteAnything |= WriteIfMissing(HardModeKey, HardMode);

        if (wroteAnything)
        {
            PlayerPrefs.Save();
            DevLog.Info("Settings: missing keys filled with defaults");
        }
    }

    public static void WriteAll()
    {
        PlayerPrefs.SetFloat(VolumeKey, Volume);
        PlayerPrefs.SetFloat(FramerateLimitKey, FramerateLimit);
        PlayerPrefs.SetFloat(MouseSensitivityKey, MouseSensitivity);
        PlayerPrefs.SetFloat(BaseFovKey, BaseFov);
        PlayerPrefs.SetFloat(CameraMotionKey, CameraMotion);
        PlayerPrefs.SetFloat(ScreenShakeKey, ScreenShake);
        PlayerPrefs.SetInt(HardModeKey, HardMode);
        PlayerPrefs.Save();
    }

    private static bool WriteIfMissing(string key, float value)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return false;
        }

        PlayerPrefs.SetFloat(key, value);
        return true;
    }

    private static bool WriteIfMissing(string key, int value)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return false;
        }

        PlayerPrefs.SetInt(key, value);
        return true;
    }
}
