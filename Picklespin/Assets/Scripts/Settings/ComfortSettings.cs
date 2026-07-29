using UnityEngine;

// One component for the options menu covering FOV, camera motion and screen shake.
//
// In-engine setup per option (same pattern as the Volume slider):
//  1. duplicate an existing settings slider row
//  2. on the Slider, set PlayerPrefsSliderManager's settingName to the pref key
//     ("BaseFOV" / "CameraBobStrenght" / "ScreenShakeStrenght") so its position persists
//  3. point the Slider's OnValueChanged (dynamic float) at the matching method below
//
// FOV slider range: degrees (suggested 60–110). Strength sliders: 0–100, like Volume.
// The receiving systems read the same pref keys on Start, so the menu scene can set
// them before the arena is ever loaded.
public class ComfortSettings : MonoBehaviour
{
    public void SetBaseFOV(float degrees)
    {
        PlayerPrefs.SetFloat("BaseFOV", degrees);
        if (DynamicFOV.instance)
        {
            DynamicFOV.instance.SetBaseFOV(degrees);
        }
    }

    // one "camera motion" comfort dial: scales CameraBob's bob offset, DynamicFOV's
    // speed-driven FOV punch, and CameraSkewController's movement-direction tilt together,
    // so reducing it tones down every camera effect caused by moving, in one step
    public void SetCameraBobStrength(float sliderValue)
    {
        PlayerPrefs.SetFloat("CameraBobStrenght", sliderValue);
        float normalized = sliderValue * 0.01f;

        if (CameraBob.instance)
        {
            CameraBob.instance.SetStrength(normalized);
        }
        if (DynamicFOV.instance)
        {
            DynamicFOV.instance.SetSpeedFovStrength(normalized);
        }
        if (CameraSkewController.instance)
        {
            CameraSkewController.instance.SetStrength(normalized);
        }
    }

    public void SetScreenShakeStrength(float sliderValue)
    {
        PlayerPrefs.SetFloat("ScreenShakeStrenght", sliderValue);
        if (CameraShakeManagerV2.instance)
        {
            CameraShakeManagerV2.instance.SetStrength(sliderValue * 0.01f);
        }
    }
}
