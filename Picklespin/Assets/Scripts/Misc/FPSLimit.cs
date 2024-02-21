using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    [System.Obsolete]
    void Start()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate-1;
       // Debug.Log("monitor's refresh rate is" + Screen.currentResolution.refreshRate + "hz");
    }

}