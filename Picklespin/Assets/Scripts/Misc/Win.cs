using FMODUnity;
using UnityEngine;

public class Win : MonoBehaviour
{
    public GameObject winScreen;
    public EventReference winEvent;

    public void PlayerWin()
    {
        RuntimeManager.PlayOneShot(winEvent);
        winScreen.SetActive(true);
        Time.timeScale = 0;
    }

}
