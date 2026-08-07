using DG.Tweening;
using UnityEngine;

public class Win : MonoBehaviour  //WHEN ESCAPING THRU THE PORTAL
{
    public static Win instance;

    public GameObject winScreen;

    private readonly float slowdownDuration = 0.2f;

    private MenuScreen screen;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;

        screen = MenuScreen.Of(winScreen);
        if (!screen) DevLog.Error($"{nameof(Win)}: no win screen wired in - escaping puts nothing on screen", this);
    }

    public void WinFunction()
    {
        if (Pause.instance) Pause.instance.UnpauseGame();
        PauseGate.Block(this);
        PlayerControlLock.Set(this, true);

        if (screen) screen.Open();

        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, slowdownDuration)
               .SetEase(Ease.OutExpo)
               .SetUpdate(UpdateType.Normal, true)
               .SetLink(gameObject)
               .OnComplete(() => Time.timeScale = 0f);
        //save game here
    }
}
