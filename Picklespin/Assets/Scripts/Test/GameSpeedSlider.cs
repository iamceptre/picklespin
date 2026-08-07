using UnityEngine;
using UnityEngine.UI;

// The methods stay in every build so the buttons wired to them still resolve; only the bodies
// are stripped, or a release build logs a missing-listener error every time one is pressed.
public class GameSpeedSlider : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private Slider me;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        me = GetComponent<Slider>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        Show();
        DevLog.Info($"{nameof(GameSpeedSlider)} is live: drag it to scale game speed up to 2x", this);
    }

#endif

    public void ApplyNewSpeed()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Time.timeScale = me.value * 2f;
#endif
    }

    public void Show()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
#endif
    }
}
