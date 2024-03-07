using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    [System.Obsolete]
    void Start()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate-1;
        //Application.targetFrameRate = 60;
    }

}