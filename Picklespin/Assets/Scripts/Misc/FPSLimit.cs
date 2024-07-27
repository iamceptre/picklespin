using UnityEngine;

public class FPSLimit : MonoBehaviour
{

    public static FPSLimit instance { get; private set; }

    [Range(29, 240)]
    [Tooltip("when set to 0, sets fps to monitors rate")]

    public float framerateLimit;
    public int monitorRefreshRate;

    void Start()
    {
        LoadSavedSetting();
        SetFramerate();
    }

    private void Awake()
    {
        GetMonitorRefreshRate();
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }


    private void LoadSavedSetting()
    {
        framerateLimit = PlayerPrefs.GetFloat("FramerateLimit");
    }

    public void SetFramerate()
    {

        QualitySettings.vSyncCount = 0;

        if (framerateLimit <= 29)
        {
            //Debug.Log("set auto fps limit");
            Application.targetFrameRate = monitorRefreshRate;
            return;
        }

        if (framerateLimit >= 240)
        {
            Application.targetFrameRate = -1;
            //Debug.Log("fps set to no limit");
            return;
        }

        Application.targetFrameRate = (int)framerateLimit;
        //Debug.Log("manual fps limit set to " + framerateLimit);
    }


    private void GetMonitorRefreshRate()
    {
        Resolution currentResolution = Screen.currentResolution;
        if (monitorRefreshRate == 0)
        {
            monitorRefreshRate = (int)currentResolution.refreshRateRatio.value;
        }
    }

}