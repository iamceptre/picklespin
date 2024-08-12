using UnityEngine;

public class SetFullscreen : MonoBehaviour
{
    public void Set(bool state)
    {
        Screen.fullScreen = state;
    }
}
