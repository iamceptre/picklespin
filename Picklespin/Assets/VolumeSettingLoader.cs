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
        readenMasterVolume = PlayerPrefs.GetFloat("Volume") * 0.01f;

        if (readenMasterVolume != 0)
        {
            LoadVolumeSetting(readenMasterVolume);
        }
        else
        {
            RefreshVolumeOutsidePlayerPrefs();
        }

    }


    public void RefreshVolumeOutsidePlayerPrefs()
    {
        LoadVolumeSetting(masterVolumeSlider.value * 0.01f);
    }


    public void LoadVolumeSetting(float volume)
    {
        //Debug.Log("set volume to " + volume);
        master.setVolume(volume);
    }


}
