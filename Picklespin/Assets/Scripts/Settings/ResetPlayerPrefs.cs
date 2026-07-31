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

    private const float DefaultVolume = 100f;
    private const float DefaultFramerateLimit = 100f;
    private const float DefaultSensitivity = 100f;
    private const float DefaultFov = 100f;
    private const float DefaultStrength = 100f; // slider units, 0-100

    public void Do()
    {
        PlayerPrefs.DeleteAll();

        // these keys fall back to a *derived* value when missing, not a fixed default,
        // so the real defaults have to be written back for the next read to reset them
        PlayerPrefs.SetFloat("Volume", DefaultVolume);
        PlayerPrefs.SetFloat("FramerateLimit", DefaultFramerateLimit);
        PlayerPrefs.SetFloat("MouseSensitivity", DefaultSensitivity);
        PlayerPrefs.SetFloat("BaseFOV", DefaultFov);
        PlayerPrefs.SetFloat("CameraBobStrenght", DefaultStrength);
        PlayerPrefs.SetFloat("ScreenShakeStrenght", DefaultStrength);

        // setting .value fires OnValueChanged, which re-applies live through the
        // existing slider wiring
        if (volumeSlider) volumeSlider.value = DefaultVolume;
        if (sensitivitySlider) sensitivitySlider.value = DefaultSensitivity;
        if (fovSlider) fovSlider.value = DefaultFov;
        if (cameraBobSlider) cameraBobSlider.value = DefaultStrength;
        if (screenShakeSlider) screenShakeSlider.value = DefaultStrength;

        // the fps slider doesn't apply live, so the limit is reapplied directly
        if (fpsSlider) fpsSlider.value = DefaultFramerateLimit;
        if (FPSLimit.instance)
        {
            FPSLimit.instance.framerateLimit = DefaultFramerateLimit;
            FPSLimit.instance.SetFramerate();
        }

        // in case these systems are alive in this scene too (an in-game pause menu)
        if (CameraBob.instance) CameraBob.instance.SetStrength(1f);
        if (DynamicFOV.instance)
        {
            DynamicFOV.instance.SetBaseFOV(DefaultFov);
            DynamicFOV.instance.SetSpeedFovStrength(1f);
        }
        if (CameraShakeManagerV2.instance) CameraShakeManagerV2.instance.SetStrength(1f);
    }
}
