using UnityEngine;

public class Death : MonoBehaviour
{
    public static Death instance;

    public GameObject deathScreen;

    private MenuScreen screen;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;

        screen = MenuScreen.Of(deathScreen);
        if (!screen) DevLog.Error($"{nameof(Death)}: no death screen wired in - dying puts nothing on screen", this);
    }

    public void PlayerDeath()
    {
        if (AudioSnapshotManager.Instance) AudioSnapshotManager.Instance.EnableSnapshot("Deathscreen");

        if (Pause.instance) Pause.instance.UnpauseGame();
        PauseGate.Block(this);
        PlayerControlLock.Set(this, true);

        if (screen) screen.Open();
        Time.timeScale = 0;
    }
}
