using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellInventorySlot : MonoBehaviour
{
    [SerializeField, Tooltip("the slot's own frame image")]
    private Image frame;
    [SerializeField, Tooltip("child image the spell sprite goes into")]
    private Image icon;
    [SerializeField, Tooltip("child label showing the digit key that selects this slot")]
    private TMP_Text number;
    [SerializeField, Tooltip("child overlay image drawn over the icon while the spell is obtainable but not owned")]
    private Image lockedTint;

    [Header("Feedback (children of this slot - image kept disabled between plays)")]
    [SerializeField, Tooltip("lock badge flashed when the locked slot's digit is pressed")]
    private Image lockBadge;
    [SerializeField, Tooltip("light sweep played when the spell unlocks")]
    private Image unlockLight;
    [SerializeField, Tooltip("aura pop played when the unlocked spell is selected")]
    private Image selectedAura;

    public RectTransform Rect { get; private set; }
    public CanvasGroup Group { get; private set; }

    private static readonly string[] digits = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    private static readonly Color dimmedIconColor = GameColors.Dimmed;

    private RectTransform numberRect;
    private float numberStartY;
    private RectTransform lockBadgeRect;
    private RectTransform unlockLightRect;
    private RectTransform selectedAuraRect;
    private Vector3 lockBadgeScale;
    private Vector3 unlockLightScale;
    private Vector3 selectedAuraScale;

    private void Awake()
    {
        Rect = (RectTransform)transform;
        Group = TryGetComponent(out CanvasGroup group) ? group : gameObject.AddComponent<CanvasGroup>();

        frame = AdoptOwnPart(frame);
        icon = AdoptOwnPart(icon);
        number = AdoptOwnPart(number);
        lockedTint = AdoptOwnPart(lockedTint);
        lockBadge = AdoptOwnPart(lockBadge);
        unlockLight = AdoptOwnPart(unlockLight);
        selectedAura = AdoptOwnPart(selectedAura);

        ActivatePart(icon);
        ActivatePart(number);
        ActivatePart(lockedTint);
        lockBadgeRect = BadgeFX.Prepare(lockBadge, out lockBadgeScale);
        unlockLightRect = BadgeFX.Prepare(unlockLight, out unlockLightScale);
        selectedAuraRect = BadgeFX.Prepare(selectedAura, out selectedAuraScale);

        numberRect = number.rectTransform;
        numberStartY = numberRect.localPosition.y;
    }

    private T AdoptOwnPart<T>(T wired) where T : Component
    {
        if (!wired || wired.transform.IsChildOf(transform)) return wired;
        T[] candidates = GetComponentsInChildren<T>(true);
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i].name == wired.name) return candidates[i];
        }
        DevLog.Warn($"{nameof(SpellInventorySlot)}: {wired.name} is wired from outside the slot and no child matches its name - wire the slot's own child instead", this);
        return wired;
    }

    private static void ActivatePart(Component part)
    {
        if (part && !part.gameObject.activeSelf) part.gameObject.SetActive(true);
    }

    public void Assign(Sprite sprite) => icon.sprite = sprite;

    public void SetNumber(int slotNumber) =>
        number.text = slotNumber >= 1 && slotNumber <= digits.Length ? digits[slotNumber - 1] : slotNumber.ToString();

    public void ApplyState(bool owned, bool selected)
    {
        frame.color = selected ? GameColors.Neutral : GameColors.Ghost;
        icon.enabled = true;
        icon.color = selected ? GameColors.Neutral : dimmedIconColor;
        if (lockedTint) lockedTint.enabled = !owned;
    }

    public void NumberBump()
    {
        numberRect.DOKill();
        Vector3 position = numberRect.localPosition;
        numberRect.localPosition = new Vector3(position.x, numberStartY, position.z);
        numberRect.DOLocalMoveY(numberStartY - 10, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    public void PlayDeny() => BadgeFX.PlayDeny(lockBadge, lockBadgeRect, lockBadgeScale);

    public void PlaySelectedAura() => BadgeFX.PlaySelectedAura(selectedAura, selectedAuraRect, selectedAuraScale);

    public void PlayUnlockFX()
    {
        icon.DOKill();
        icon.enabled = true;
        icon.DOFade(0, 0);
        icon.DOFade(1, 0.5f);

        if (!unlockLight) return;
        unlockLight.DOKill();
        unlockLightRect.DOKill();
        unlockLight.enabled = true;
        unlockLightRect.localScale = Vector3.zero;
        unlockLightRect.DOScaleY(unlockLightScale.y, 0.3f).SetEase(Ease.OutExpo);
        unlockLightRect.DOScaleX(unlockLightScale.x, 1).SetEase(Ease.OutExpo);
        unlockLight.DOFade(1, 0.1f)
            .OnComplete(() => unlockLight.DOFade(0, 4)
            .OnComplete(() => unlockLight.enabled = false));
    }
}
