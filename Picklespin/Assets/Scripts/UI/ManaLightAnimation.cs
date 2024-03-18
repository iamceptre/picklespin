using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ManaLightAnimation : MonoBehaviour
{

    private Image manaLight;
    private RectTransform rectTransform;

    private void Awake()
    {
        manaLight = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void LightAnimation()
    {
        manaLight.enabled = true;
        manaLight.DOKill();
        rectTransform.localScale = new Vector3(0, 0, 1);
        rectTransform.DOScaleY(3, 1).SetEase(Ease.OutExpo);
        rectTransform.DOScaleX(4, 1).SetEase(Ease.OutExpo);
        manaLight.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        manaLight.DOFade(0, 1.37f).SetEase(Ease.InSine).OnComplete(DisableMe);
        rectTransform.DOScale(1, 1.37f).SetEase(Ease.InSine);
    }

    private void DisableMe()
    {
        manaLight.enabled = false;
    }

}
