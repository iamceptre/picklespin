using UnityEngine;

// Wiring a new option: set the Slider's PlayerPrefsSliderManager.settingName to the
// pref key, then point its OnValueChanged (dynamic float) at the method here. FOV is
// in degrees, the strength sliders are 0-100 like Volume.
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

    // one dial for every camera effect caused by moving: bob, FOV punch and tilt
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
