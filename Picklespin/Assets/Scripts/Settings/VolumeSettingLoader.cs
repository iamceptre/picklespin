using UnityEngine;
using FMOD.Studio;
using UnityEngine.UI;

public class VolumeSettingLoader : MonoBehaviour
{

    private Bus master;
    private float readenMasterVolume;
    [SerializeField] private Slider masterVolumeSlider;

    private void Awake()
    {
        master = FMODUnity.RuntimeManager.GetBus("bus:/");
    }

    private void Start()
    {
        readenMasterVolume = PlayerPrefs.GetFloat(SettingsDefaults.VolumeKey, SettingsDefaults.Volume) * 0.01f;
        LoadVolumeSetting(readenMasterVolume);
    }

    public void RefreshVolumeOutsidePlayerPrefs()
    {
        LoadVolumeSetting(masterVolumeSlider.value * 0.01f);
    }

    public void LoadVolumeSetting(float volume)
    {
        master.setVolume(volume);
    }

}
