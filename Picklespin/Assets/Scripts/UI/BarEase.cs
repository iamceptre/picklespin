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

    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    private void Start()
    {
        me.value = sliderToFollow.value;
        if (easeFill) easeFill.enabled = true;
    }

    private void Update()
    {
        float realValue = sliderToFollow.value;
        if (Mathf.Approximately(me.value, realValue)) return;

        if (realValue > me.value)
        {
            KillTween();
            me.value = realValue;
            if (easeFill) easeFill.enabled = false;
        }
        else
        {
            KillTween();
            if (easeFill) easeFill.enabled = true;
            currentTween = me.DOValue(realValue, easeDuration)
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    me.value = realValue;
                    if (easeFill) easeFill.enabled = false;
                });
        }
    }

    private void KillTween()
    {
        if (currentTween != null && currentTween.IsActive()) currentTween.Kill();
        currentTween = null;
    }

    public void FadeOut()
    {
        if (easeFill) easeFill.DOFade(0f, 0.5f);
    }

    public void FadeIn()
    {
        if (easeFill) easeFill.DOFade(1f, 0.2f);
    }
}