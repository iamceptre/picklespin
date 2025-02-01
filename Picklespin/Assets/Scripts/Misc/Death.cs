using UnityEngine;
using FMODUnity;

public class Death : MonoBehaviour
{
    public static Death instance;
    public GameObject deathScreen;

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
        AudioSnapshotManager.Instance.EnableSnapshot("Deathscreen");
        deathScreen.SetActive(true);
        Time.timeScale = 0;
    }
}
