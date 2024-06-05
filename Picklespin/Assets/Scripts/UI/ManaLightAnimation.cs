using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ManaLightAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text manaPlusPlus;
    private RectTransform manaPlusPlusRect;
    private float manaPlusPlusStartingPos;

    private Image manaLight;
    private RectTransform rectTransform;

    private WaitForSeconds waitBeforeFadingPlusPlus = new WaitForSeconds(2);

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

    public void LightAnimation(float howMuchWasGiven, bool maxxed)
    {
        manaLight.enabled = true;
        manaLight.DOKill();
        rectTransform.localScale = new Vector3(0, 0, 1);
        rectTransform.DOScaleY(3, 1).SetEase(Ease.OutExpo);
        rectTransform.DOScaleX(1.5f, 1).SetEase(Ease.OutExpo);
        manaLight.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);

        if (manaPlusPlus != null)
        {
            ManaPlusPlusAnimation(howMuchWasGiven, maxxed);
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

    private void ManaPlusPlusAnimation(float howMuchWasGiven, bool maxxed)
    {
        manaPlusPlus.color = new Color(manaPlusPlus.color.r, manaPlusPlus.color.g, manaPlusPlus.color.b, 0);
        manaPlusPlus.enabled = true;
        manaPlusPlus.color = new Color(manaPlusPlus.color.r, manaPlusPlus.color.g, manaPlusPlus.color.b, 0);

        if (maxxed)
        {
            manaPlusPlus.text = "<b>+ " + howMuchWasGiven + "</b> *";
        }
        else
        {
            manaPlusPlus.text = "<b>+ " + howMuchWasGiven;
        }

        manaPlusPlusRect.localPosition = new Vector2(manaPlusPlusRect.localPosition.x, manaPlusPlusStartingPos);
        manaPlusPlus.DOKill();
        manaPlusPlusRect.DOKill();
        //manaPlusPlus.DOFade(0, 0);
        manaPlusPlus.color = new Color(manaPlusPlus.color.r, manaPlusPlus.color.g, manaPlusPlus.color.b, 0);
        manaPlusPlus.DOFade(1, 0.4f).OnComplete(() =>
        {
            StopAllCoroutines();
            StartCoroutine(WaitAndFadeOut());
        });
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return waitBeforeFadingPlusPlus;
        ManaPlusPlusFadeOut();
    }

    private void ManaPlusPlusFadeOut()
    {
        manaPlusPlusRect.DOLocalMoveY(manaPlusPlusStartingPos + 50, 2).SetEase(Ease.InSine);
        manaPlusPlus.DOFade(0, 2).SetEase(Ease.InSine).OnComplete(DisableManaPlusPlus);
    }

    private void DisableManaPlusPlus()
    {
        manaPlusPlus.enabled = false;
    }

}
