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
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(3, 0.2f).SetEase(Ease.InSine);
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
