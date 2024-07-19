using FMODUnity;
using UnityEngine;

public class Win : MonoBehaviour  //WHEN ESCAPING THRU THE PORTAL
{
    public static Win instance;

    public GameObject winScreen;
    private CanvasGroupOpacityAnimator winScreenFadeIn;

    public EventReference winFmodEvent;

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
        winScreenFadeIn = winScreen.GetComponent<CanvasGroupOpacityAnimator>();
    }

    public void WinFunction()
    {
        RuntimeManager.PlayOneShot(winFmodEvent);
        winScreen.SetActive(true);
        winScreenFadeIn.enabled = true;
        //Time.timeScale = 0;
    }

}
