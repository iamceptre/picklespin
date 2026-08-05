using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class BadgeFX
{
    public static RectTransform Prepare(Image badge, out Vector3 authoredScale)
    {
        if (!badge)
        {
            authoredScale = Vector3.one;
            return null;
        }
        if (!badge.gameObject.activeSelf) badge.gameObject.SetActive(true);
        badge.enabled = false;
        RectTransform rect = badge.rectTransform;
        authoredScale = rect.localScale;
        return rect;
    }

    public static void Play(Image badge, RectTransform rect, Vector3 authoredScale,
        float startScaleRatio, float scaleTime,
        float peakAlpha, float fadeInTime, float fadeOutTime)
    {
        if (!badge) return;
        badge.DOKill();
        rect.DOKill();
        badge.enabled = true;
        badge.color = GameColors.ClearWhite;
        rect.localScale = authoredScale * startScaleRatio;
        rect.DOScale(authoredScale, scaleTime);
        badge.DOFade(peakAlpha, fadeInTime)
            .OnComplete(() => badge.DOFade(0f, fadeOutTime)
            .OnComplete(() => badge.enabled = false));
    }
}
