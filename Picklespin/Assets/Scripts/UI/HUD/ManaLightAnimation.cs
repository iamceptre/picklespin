using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

public class ManaLightAnimation : MonoBehaviour
{
    [SerializeField, Tooltip("which bar this light sits on - it glows in that bar's colour, set in PlayerClassHud")]
    private HudResource bar = HudResource.Magicka;
    [SerializeField] private TMP_Text manaPlusPlus;
    private RectTransform manaPlusPlusRect;
    private float manaPlusPlusStartingPos;
    private Image manaLight;
    private RectTransform rectTransform;
    private readonly WaitForSeconds waitBeforeFadingPlusPlus = new(2);
    private readonly StringBuilder sb = new();
    private Color originalColor;
    private Color manaPlusPlusStartColor = GameColors.Neutral;

    private readonly Color negativeGlowColor = GameColors.NegativeGlow;

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
            manaPlusPlusStartColor = manaPlusPlus.color;
        }
    }

    public void LightAnimation(float howMuchWasGiven, bool maxxed, HudResource resource)
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
            manaLight.color = GlowColor();
        }
        rectTransform.DOScaleY(3, scaleDuration).SetEase(Ease.OutExpo);
        rectTransform.DOScaleX(1.5f, scaleDuration).SetEase(Ease.OutExpo);
        manaLight.DOFade(1, fadeInDuration).SetEase(Ease.InSine).OnComplete(() =>
        {
            FadeOut(fadeOutDuration);
        });
        if (manaPlusPlus != null) ManaPlusPlusAnimation(howMuchWasGiven, maxxed, resource);
    }

    private Color GlowColor()
    {
        if (!PlayerClassHud.Instance) return originalColor;

        Color barColor = PlayerClassHud.Instance.BarLightColor(bar);
        barColor.a = originalColor.a;
        return barColor;
    }

    private void FadeOut(float duration)
    {
        manaLight.DOFade(0, duration).SetEase(Ease.OutSine).OnComplete(() => manaLight.enabled = false);
        rectTransform.DOScale(1, duration).SetEase(Ease.InSine);
    }

    private void ManaPlusPlusAnimation(float howMuchWasGiven, bool maxxed, HudResource resource)
    {
        sb.Clear();
        sb.Append("<b>");
        sb.Append(howMuchWasGiven.ToString("+#;-#;0"));
        if (maxxed) sb.Append("</b> *");
        else sb.Append("</b>");
        manaPlusPlus.text = sb.ToString();
        manaPlusPlus.enabled = true;

        Color textColor = manaPlusPlusStartColor;
        if (PlayerClassHud.Instance && PlayerClassHud.Instance.TryGetResourceColor(resource, out Color barColor))
        {
            textColor = barColor;
        }
        manaPlusPlus.color = textColor.WithAlpha(0f);
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
