using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class AngelHealBoostLight : MonoBehaviour
{

    private Image lightFX;
    private RectTransform rectTransform;

    private void Awake()
    {
        lightFX = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void LightAnimation()
    {
        lightFX.enabled = true;
        lightFX.DOKill();
        rectTransform.localScale = new Vector3(0, 0, 1);
        rectTransform.DOScaleY(3, 1).SetEase(Ease.OutExpo);
        rectTransform.DOScaleX(3, 1).SetEase(Ease.OutExpo);
        lightFX.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        lightFX.DOFade(0, 1.37f).SetEase(Ease.OutSine).OnComplete(DisableMe);
        rectTransform.DOScale(1, 1.37f).SetEase(Ease.InSine);
    }

    private void DisableMe()
    {
        lightFX.enabled = false;
    }

}
