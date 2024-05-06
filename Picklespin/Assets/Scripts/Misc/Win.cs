using FMODUnity;
using UnityEngine;

public class Win : MonoBehaviour
{
    public static Win instance;

    public GameObject winScreen;
    public EventReference winEvent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

    }

    public void WinFunction()
    {
        RuntimeManager.PlayOneShot(winEvent);
        winScreen.SetActive(true);
        Time.timeScale = 0;
    }

}
