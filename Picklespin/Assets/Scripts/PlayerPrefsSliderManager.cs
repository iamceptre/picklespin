using UnityEngine;
using UnityEngine.UI;

public class PlayerPrefsSliderManager : MonoBehaviour
{
    private Slider _slider;
    [SerializeField] private string settingName = "setMyName";

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        LoadSetting();
    }

    public void SaveSetting() {

        if (settingName != null && _slider != null)
        {
            PlayerPrefs.SetFloat(settingName, _slider.value);
        }
            //Debug.Log("saved " + settingName + " to " + _slider.value);
            return;
    }


    public void LoadSetting()
    {
        if (settingName != null && _slider != null)
        {
            _slider.value = PlayerPrefs.GetFloat(settingName, _slider.value);
        }
        //do rest of the loading in coresponding scripts like MouseLook, VolumeSettingLoader etc
    }

}
