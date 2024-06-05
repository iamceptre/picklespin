using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    [Range(0,300)]
    [Tooltip("when set to 0, caps fps at monitor's refresh rate")]
    public int framerateLimit = 0;

    void Start()
    {
        SetFramerate();
    }



    public void SetFramerate()
    {
        Resolution currentResolution = Screen.currentResolution;

        if (framerateLimit == 0)
        {
            Application.targetFrameRate = Mathf.FloorToInt((float)currentResolution.refreshRateRatio.value - 1);
            //Debug.Log("fps cap set to: " + Application.targetFrameRate +" (which is your monitor refresh rate)");
        }
        else
        {
            Application.targetFrameRate = framerateLimit;
        }
    }

}