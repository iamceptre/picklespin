using UnityEngine;

public class BulkSettingsManager : MonoBehaviour
{
    [SerializeField, Tooltip("left empty, every PlayerPrefs slider under this object is saved - which is what a settings screen dropped into a new scene needs")]
    private PlayerPrefsSliderManager[] playerPrefsSlider;

    private void Awake()
    {
        if (playerPrefsSlider == null || playerPrefsSlider.Length == 0)
        {
            playerPrefsSlider = GetComponentsInChildren<PlayerPrefsSliderManager>(true);
        }
    }

    public void SaveAllSettings()
    {
        for (int i = 0; i < playerPrefsSlider.Length; i++)
        {
            if (playerPrefsSlider[i]) playerPrefsSlider[i].SaveSetting();
        }
    }
}
