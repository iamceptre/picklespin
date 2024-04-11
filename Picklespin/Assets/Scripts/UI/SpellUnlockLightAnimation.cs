using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SpellUnlockedLightAnimation : MonoBehaviour
{
    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void LightAnimation()
    {
        image.enabled = true;
        image.DOKill();
        rectTransform.localScale = Vector3.zero;
        //rectTransform.DOScale(1f, 1).SetEase(Ease.OutExpo);
        rectTransform.DOScaleY(1f, 0.3f).SetEase(Ease.OutExpo);
        rectTransform.DOScaleX(1f, 1).SetEase(Ease.OutExpo);
        image.DOFade(1, 0.1f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        image.DOFade(0, 1.37f).SetEase(Ease.OutSine).OnComplete(DisableMe);
    }

    private void DisableMe()
    {
        image.enabled = false;
    }

}
