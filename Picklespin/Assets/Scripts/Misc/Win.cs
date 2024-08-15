using UnityEngine;

public class Win : MonoBehaviour  //WHEN ESCAPING THRU THE PORTAL
{
    public static Win instance;

    public GameObject winScreen;
    private CanvasGroupOpacityAnimator winScreenFadeIn;

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
        winScreen.SetActive(true);
        winScreenFadeIn.enabled = true;
        //save game here
    }



}
