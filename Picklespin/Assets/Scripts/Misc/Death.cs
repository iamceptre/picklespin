using UnityEngine;
using FMODUnity;

public class Death : MonoBehaviour
{
    public static Death instance;

    public GameObject deathScreen;
    public EventReference deathEvent;

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


    public void PlayerDeath()
    {
        RuntimeManager.PlayOneShot(deathEvent);
        deathScreen.SetActive(true);
        Time.timeScale = 0;
    }
}
