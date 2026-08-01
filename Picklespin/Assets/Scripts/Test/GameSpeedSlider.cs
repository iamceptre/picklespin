using UnityEngine;
using UnityEngine.UI;

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

    private void Start() => Show();

    public void ApplyNewSpeed()
    {
        Time.timeScale = me.value*2;
    }

    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

#endif
}
