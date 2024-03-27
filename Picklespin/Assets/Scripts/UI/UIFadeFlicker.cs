using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIFadeFlicker : MonoBehaviour
{

    private Image me;
    private float startingAlpha;
    private bool isFlickering;

    private void Awake()
    {
        me = GetComponent<Image>();
        startingAlpha = me.color.a;
    }

    public void StartFlicker()
    {
        me.DOKill();
        if (!isFlickering) {
            FadeIn();
        }
    }

    public void StopFlicker()
    {
        me.DOKill();
        isFlickering = false;
        me.color = new Color(me.color.r, me.color.g, me.color.b, startingAlpha);
        enabled = false;
    }

    private void FadeIn()
    {
        me.DOFade(1, 0.1f).OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        me.DOFade(0f, 0.1f).OnComplete(FadeIn);
    }

}
