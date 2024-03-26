using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class NoManaLightAnimation : MonoBehaviour
{
    private Image manaLight;

    private void Awake()
    {
        manaLight = GetComponent<Image>();
    }

    public void LightAnimation()
    {
        manaLight.enabled = true;
        manaLight.DOKill();
        manaLight.DOFade(1, 0.1f).SetEase(Ease.InSine).OnComplete(FadeOut);
    }

    private void FadeOut()
    {
        manaLight.DOFade(0, 0.2f).SetEase(Ease.InSine).OnComplete(DisableMe);
    }

    private void DisableMe()
    {
        manaLight.enabled = false;
    }

}
