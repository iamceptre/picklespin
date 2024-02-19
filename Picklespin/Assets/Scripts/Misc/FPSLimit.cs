using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    [System.Obsolete]
    void Start()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        Debug.Log("monitor set to" + Screen.currentResolution.refreshRate + "hz");
    }

}