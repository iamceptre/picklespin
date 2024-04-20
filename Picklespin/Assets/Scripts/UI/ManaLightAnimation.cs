using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class ManaLightAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text manaPlusPlus;
    private RectTransform manaPlusPlusRect;
    private float manaPlusPlusStartingPos;

    private Image manaLight;
    private RectTransform rectTransform;

    private void Awake()
    {
        manaLight = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (manaPlusPlus != null) {
            manaPlusPlusRect = manaPlusPlus.GetComponent<RectTransform>();
            manaPlusPlus.enabled = false;
            manaPlusPlusStartingPos = manaPlusPlusRect.localPosition.y;
        }
    }

    public void LightAnimation(float howMuchWasGiven)
    {
        manaLight.enabled = true;
        manaLight.DOKill();
        rectTransform.localScale = new Vector3(0, 0, 1);
        rectTransform.DOScaleY(3, 1).SetEase(Ease.OutExpo);
        rectTransform.DOScaleX(1.5f, 1).SetEase(Ease.OutExpo);
        manaLight.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);

        if (manaPlusPlus != null)
        {
            ManaPlusPlusAnimation(howMuchWasGiven);
        }

    }

    private void FadeOut()
    {
        manaLight.DOFade(0, 1.37f).SetEase(Ease.OutSine).OnComplete(DisableMe);
        rectTransform.DOScale(1, 1.37f).SetEase(Ease.InSine);
    }

    private void DisableMe()
    {
        manaLight.enabled = false;
    }

    private void ManaPlusPlusAnimation(float howMuchWasGiven)
    {
        manaPlusPlus.enabled = true;
        manaPlusPlus.text = "<b>+ " + howMuchWasGiven;
        manaPlusPlusRect.localPosition = new Vector2(manaPlusPlusRect.localPosition.x, manaPlusPlusStartingPos);
        manaPlusPlus.DOKill();
        manaPlusPlusRect.DOKill();
        manaPlusPlus.DOFade(0, 0);
        manaPlusPlusRect.DOLocalMoveY(manaPlusPlusStartingPos + 50, 2);
        manaPlusPlus.DOFade(1, 0.2f).OnComplete(ManaPlusPlusFadeOut);
    }

    private void ManaPlusPlusFadeOut()
    {
        manaPlusPlus.DOFade(0, 2).OnComplete(DisableManaPlusPlus);
    }

    private void DisableManaPlusPlus()
    {
        manaPlusPlus.enabled = false;
    }

}
