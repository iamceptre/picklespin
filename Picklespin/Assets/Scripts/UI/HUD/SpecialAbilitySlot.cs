using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpecialAbilitySlot : MonoBehaviour
{
    [SerializeField, Tooltip("the slot's own frame image")]
    private Image frame;
    [SerializeField, Tooltip("child image the current ability's sprite goes into")]
    private Image icon;
    [SerializeField, Tooltip("child overlay image drawn over the icon while the ability cannot be used right now")]
    private Image lockedTint;

    [Header("Feedback (child of this slot - image kept disabled between plays)")]
    [SerializeField, Tooltip("lock badge flashed when the ability is triggered while it can't be used")]
    private Image lockBadge;
    [SerializeField, Tooltip("aura pop played when the ability is actually used")]
    private Image selectedAura;

    [SerializeField, Tooltip("fade time for the locked overlay appearing/disappearing")]
    private float lockFadeTime = 0.2f;

    private static readonly Color dimmedIconColor = GameColors.Dimmed;

    private RectTransform lockBadgeRect;
    private Vector3 lockBadgeScale;
    private RectTransform selectedAuraRect;
    private Vector3 selectedAuraScale;
    private float lockedTintAlpha;
    private bool? shownAsLocked;

    private void Awake()
    {
        if (icon && !icon.gameObject.activeSelf) icon.gameObject.SetActive(true);
        if (lockedTint)
        {
            if (!lockedTint.gameObject.activeSelf) lockedTint.gameObject.SetActive(true);
            lockedTintAlpha = lockedTint.color.a;
            lockedTint.enabled = false;
        }
        lockBadgeRect = BadgeFX.Prepare(lockBadge, out lockBadgeScale);
        selectedAuraRect = BadgeFX.Prepare(selectedAura, out selectedAuraScale);
    }

    public void Assign(Sprite sprite)
    {
        if (!icon) return;
        icon.sprite = sprite;
        icon.enabled = sprite;
    }

    public void ApplyState(bool usable)
    {
        if (frame) frame.color = usable ? GameColors.Neutral : GameColors.Ghost;
        if (icon) icon.color = usable ? GameColors.Neutral : dimmedIconColor;
        if (!lockedTint) return;

        bool locked = !usable;
        if (shownAsLocked == locked) return;
        shownAsLocked = locked;

        lockedTint.DOKill();
        if (locked)
        {
            lockedTint.enabled = true;
            lockedTint.color = lockedTint.color.WithAlpha(0f);
            lockedTint.DOFade(lockedTintAlpha, lockFadeTime);
        }
        else
        {
            lockedTint.DOFade(0f, lockFadeTime).OnComplete(() => lockedTint.enabled = false);
        }
    }

    public void PlayDeny()
    {
        BadgeFX.Play(lockBadge, lockBadgeRect, lockBadgeScale,
            startScaleRatio: 0.625f, scaleTime: 0.7f,
            peakAlpha: 1f, fadeInTime: 0.2f, fadeOutTime: 0.5f);
    }

    public void PlaySelectedAura()
    {
        BadgeFX.Play(selectedAura, selectedAuraRect, selectedAuraScale,
            startScaleRatio: 0.714f, scaleTime: 0.35f,
            peakAlpha: 0.6f, fadeInTime: 0.1f, fadeOutTime: 0.3f);
    }
}
