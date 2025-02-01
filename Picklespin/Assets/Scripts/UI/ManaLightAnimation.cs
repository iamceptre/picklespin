using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using System.Runtime.CompilerServices;

public class ManaLightAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text manaPlusPlus;
    private RectTransform manaPlusPlusRect;
    private float manaPlusPlusStartingPos;
    private Image manaLight;
    private RectTransform rectTransform;
    private readonly WaitForSeconds waitBeforeFadingPlusPlus = new(2);
    private readonly StringBuilder sb = new();
    private Color originalColor;

    private Color negativeGlowColor = new(0, 0, 0, 0.38f);

    private void Awake()
    {
        manaLight = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        originalColor = manaLight.color;
        if (manaPlusPlus != null)
        {
            manaPlusPlusRect = manaPlusPlus.GetComponent<RectTransform>();
            manaPlusPlus.enabled = false;
            manaPlusPlusStartingPos = manaPlusPlusRect.localPosition.y;
        }
    }

    public void LightAnimation(float howMuchWasGiven, bool maxxed)
    {
        manaLight.enabled = true;
        manaLight.DOKill();
        rectTransform.localScale = Vector3.zero;
        float scaleDuration = 1f;
        float fadeInDuration = 0.2f;
        float fadeOutDuration = 1.37f;
        if (howMuchWasGiven < 0)
        {
            manaLight.color = negativeGlowColor;
            scaleDuration *= 0.38f;
            fadeInDuration *= 0.38f;
            fadeOutDuration *= 0.38f;
        }
        else
        {
            manaLight.color = originalColor;
        }
        rectTransform.DOScaleY(3, scaleDuration).SetEase(Ease.OutExpo);
        rectTransform.DOScaleX(1.5f, scaleDuration).SetEase(Ease.OutExpo);
        manaLight.DOFade(1, fadeInDuration).SetEase(Ease.InSine).OnComplete(() =>
        {
            FadeOut(fadeOutDuration);
        });
        if (manaPlusPlus != null) ManaPlusPlusAnimation(howMuchWasGiven, maxxed);
    }

    private void FadeOut(float duration)
    {
        manaLight.DOFade(0, duration).SetEase(Ease.OutSine).OnComplete(() => manaLight.enabled = false);
        rectTransform.DOScale(1, duration).SetEase(Ease.InSine);
    }

    private void ManaPlusPlusAnimation(float howMuchWasGiven, bool maxxed)
    {
        sb.Clear();
        sb.Append("<b>");
        sb.Append(howMuchWasGiven.ToString("+#;-#;0"));
        if (maxxed) sb.Append("</b> *");
        else sb.Append("</b>");
        manaPlusPlus.text = sb.ToString();
        manaPlusPlus.enabled = true;
        manaPlusPlus.color = new Color(manaPlusPlus.color.r, manaPlusPlus.color.g, manaPlusPlus.color.b, 0);
        manaPlusPlusRect.localPosition = new Vector2(manaPlusPlusRect.localPosition.x, manaPlusPlusStartingPos);
        manaPlusPlus.DOKill();
        manaPlusPlusRect.DOKill();
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
        manaPlusPlus.DOFade(0, 2).SetEase(Ease.InSine).OnComplete(() => manaPlusPlus.enabled = false);
    }
}
