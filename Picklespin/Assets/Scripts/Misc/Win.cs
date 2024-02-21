using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

public class Win : MonoBehaviour
{
    public Image winScreen;
    public EventReference winEvent;

    public void PlayerWin()
    {
        RuntimeManager.PlayOneShot(winEvent);
        winScreen.color = Color.white;
        Time.timeScale = 0;
    }

}
