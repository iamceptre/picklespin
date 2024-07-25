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

            PlayerPrefs.SetFloat(settingName, _slider.value);
            Debug.Log("saved " + settingName + " to " + _slider.value);
            return;
    }


    public void LoadSetting()
    {
        float readenSens = PlayerPrefs.GetFloat(settingName);

        if (readenSens <= 0)
        {
            readenSens = _slider.value;
        }

        _slider.value = readenSens;
    }

}
