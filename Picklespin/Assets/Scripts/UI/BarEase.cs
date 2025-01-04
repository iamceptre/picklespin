using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Slider))]
public class BarEase : MonoBehaviour
{
    public Slider sliderToFollow;
    [SerializeField] private Image easeFill;
    [SerializeField] private float easeDuration = 1f;

    private Slider me;
    private Tween currentTween;
    private Tween fadeTween;
    private bool easeFillVisible;

    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    private void Start()
    {
        me.value = sliderToFollow.value;
        SetEaseFillState(true);
    }

    private void Update()
    {
        float realValue = sliderToFollow.value;
        if (Mathf.Approximately(me.value, realValue)) return;

        if (realValue > me.value)
        {
            KillTween();
            me.value = realValue;
            SetEaseFillState(false, 0.2f);
        }
        else
        {
            KillTween();
            SetEaseFillState(true);
            currentTween = me.DOValue(realValue, easeDuration)
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    me.value = realValue;
                    SetEaseFillState(false, 0.2f);
                });
        }
    }

    private void KillTween()
    {
        if (currentTween != null && currentTween.IsActive()) currentTween.Kill();
        currentTween = null;
    }

    public void SetEaseFillState(bool visible, float fadeDuration = 0f)
    {
        if (easeFillVisible == visible) return;
        easeFillVisible = visible;

        if (fadeTween != null && fadeTween.IsActive()) fadeTween.Kill();
        easeFill.enabled = true;

        float targetAlpha = visible ? 1f : 0f;
        fadeTween = easeFill.DOFade(targetAlpha, fadeDuration)
            .OnComplete(() =>
            {
                if (!visible) easeFill.enabled = false;
            });
    }
}
