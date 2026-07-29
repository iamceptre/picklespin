using UnityEngine;

public class SetFullscreen : MonoBehaviour
{
    public void Set(bool state)
    {
        Screen.fullScreen = state;
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, state);
    }
}
