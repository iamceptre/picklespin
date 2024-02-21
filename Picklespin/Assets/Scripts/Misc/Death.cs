using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class Death : MonoBehaviour
{
    public Image deathScreen;
    public EventReference deathEvent;
   public void PlayerDeath()
    {
        RuntimeManager.PlayOneShot(deathEvent);
        deathScreen.color = Color.white;
        Time.timeScale = 0;
        //add some cool master channel muffle and shit
    }
}
