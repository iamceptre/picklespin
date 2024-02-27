using UnityEngine;
using FMODUnity;

public class Death : MonoBehaviour
{
    public GameObject deathScreen;
    public EventReference deathEvent;
   public void PlayerDeath()
    {
        RuntimeManager.PlayOneShot(deathEvent);
        deathScreen.SetActive(true);
        Time.timeScale = 0;
    }
}
