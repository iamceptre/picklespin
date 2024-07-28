using UnityEngine;

public class BulkSettingsManager : MonoBehaviour
{
    [SerializeField] private PlayerPrefsSliderManager[] playerPrefsSlider;


    public void SaveAllSettings()
    {
        for (int i = 0; i < playerPrefsSlider.Length; i++)
        {
            playerPrefsSlider[i].SaveSetting();
        }
    }
}
