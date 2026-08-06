using UnityEngine;
using UnityEngine.UI;

public class ResetPlayerPrefs : MonoBehaviour
{
    [Header("Optional: wire the sliders here too, to snap them back visually")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider cameraBobSlider;
    [SerializeField] private Slider screenShakeSlider;
    [SerializeField] private Toggle hardModeToggle;

    public void Do()
    {
        PlayerPrefs.DeleteAll();
        SettingsDefaults.WriteAll();
        HardMode.Set(SettingsDefaults.HardMode != 0);
        if (hardModeToggle) hardModeToggle.SetIsOnWithoutNotify(SettingsDefaults.HardMode != 0);

        // setting .value fires OnValueChanged, which re-applies live through the
        // existing slider wiring
        if (volumeSlider) volumeSlider.value = SettingsDefaults.Volume;
        if (sensitivitySlider) sensitivitySlider.value = SettingsDefaults.MouseSensitivity;
        if (fovSlider) fovSlider.value = SettingsDefaults.BaseFov;
        if (cameraBobSlider) cameraBobSlider.value = SettingsDefaults.CameraMotion;
        if (screenShakeSlider) screenShakeSlider.value = SettingsDefaults.ScreenShake;

        // the fps slider doesn't apply live, so the limit is reapplied directly
        if (fpsSlider) fpsSlider.value = SettingsDefaults.FramerateLimit;
        if (FPSLimit.instance)
        {
            FPSLimit.instance.framerateLimit = SettingsDefaults.FramerateLimit;
            FPSLimit.instance.SetFramerate();
        }

        // in case these systems are alive in this scene too (an in-game pause menu)
        if (MouselookXY.instance) MouselookXY.instance.RestoreSensitivity();
        if (CameraBob.instance) CameraBob.instance.SetStrength(1f);
        if (CameraSkewController.instance) CameraSkewController.instance.SetStrength(1f);
        if (DynamicFOV.instance)
        {
            DynamicFOV.instance.SetBaseFOV(SettingsDefaults.BaseFov);
            DynamicFOV.instance.SetSpeedFovStrength(1f);
        }
        if (CameraShakeManagerV2.instance) CameraShakeManagerV2.instance.SetStrength(1f);
    }
}
