using UnityEngine;
using UnityEngine.UI;

public class ResetPlayerPrefs : MonoBehaviour
{
    [Header("Optional: wire the 3 comfort sliders to snap them back visually here too")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider cameraBobSlider;
    [SerializeField] private Slider screenShakeSlider;

    private const float DefaultFov = 100f;
    private const float DefaultStrength = 100f; // slider units, 0-100

    public void Do()
    {
        PlayerPrefs.DeleteAll();

        // BaseFOV / CameraBobStrenght (sic) / ScreenShakeStrenght (sic) fall back to a
        // *derived* value (the camera's own serialized FOV, or "no override at all") when
        // their key is missing, not a fixed default — write the real defaults straight back
        // so the very next read (slider reload here, or the arena scene) actually resets them
        PlayerPrefs.SetFloat("BaseFOV", DefaultFov);
        PlayerPrefs.SetFloat("CameraBobStrenght", DefaultStrength);
        PlayerPrefs.SetFloat("ScreenShakeStrenght", DefaultStrength);

        // snap sliders visually if this button has them wired; setting .value fires
        // OnValueChanged, which re-applies live through the existing ComfortSettings wiring
        if (fovSlider) fovSlider.value = DefaultFov;
        if (cameraBobSlider) cameraBobSlider.value = DefaultStrength;
        if (screenShakeSlider) screenShakeSlider.value = DefaultStrength;

        // apply immediately in case these systems are alive in this scene too
        // (e.g. an in-game pause menu using the same Reset button)
        if (CameraBob.instance) CameraBob.instance.SetStrength(1f);
        if (DynamicFOV.instance)
        {
            DynamicFOV.instance.SetBaseFOV(DefaultFov);
            DynamicFOV.instance.SetSpeedFovStrength(1f);
        }
        if (CameraShakeManagerV2.instance) CameraShakeManagerV2.instance.SetStrength(1f);
    }
}
