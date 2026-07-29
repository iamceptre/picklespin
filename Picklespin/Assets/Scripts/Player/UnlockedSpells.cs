using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

// Owns spell unlock state and the inventory-bar unlock/locked/selected feedback.
// All per-spell arrays are indexed by spellID; array lengths define the spell count.
public class UnlockedSpells : MonoBehaviour
{
    public static UnlockedSpells instance { get; private set; }

    [Header("State (index = spellID)")]
    public bool[] spellUnlocked;

    [Header("Inventory Bar (index = spellID)")]
    [SerializeField] RectTransform[] invSlotRect;
    [SerializeField] Image[] spellIcon;
    [SerializeField] GameObject[] lockedSpellTint;

    [Header("Feedback Badges")]
    [SerializeField] Image spellLockedIcon;
    [SerializeField] Image alreadyUnlockedIcon;
    [SerializeField] Image spellUnlockedLight;
    [SerializeField] RectTransform currentlySelectedSlotIndicator;

    public int SpellCount => spellUnlocked.Length;
    public event System.Action<int> OnSpellUnlocked;

    private const int DuplicateUnlockManaRefund = 50;
    private const string DuplicateUnlockSound = "event:/ITEMS/POTIONS/POTION_PICKUP_BASE_LAYER";

    private RectTransform lockedRect;
    private RectTransform alreadyUnlockedRect;
    private RectTransform lightRect;
    private Ammo ammo;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this); else instance = this;
        lightRect = spellUnlockedLight.GetComponent<RectTransform>();
        lockedRect = spellLockedIcon.GetComponent<RectTransform>();
        alreadyUnlockedRect = alreadyUnlockedIcon.GetComponent<RectTransform>();
    }

    void Start()
    {
        ammo = Ammo.instance;
    }

    public bool IsUnlocked(int spellID)
    {
        return spellUnlocked[spellID];
    }

    public void UnlockASpell(int spellID)
    {
        if (spellUnlocked[spellID])
        {
            // duplicate pickup: refund as mana instead
            ammo.GiveManaToPlayer(DuplicateUnlockManaRefund, false);
            RuntimeManager.PlayOneShot(DuplicateUnlockSound);
            return;
        }

        spellUnlocked[spellID] = true;
        lockedSpellTint[spellID].SetActive(false);
        PlayUnlockLight(spellID);
        spellIcon[spellID].enabled = true;
        spellIcon[spellID].DOFade(0, 0);
        spellIcon[spellID].DOFade(1, 0.5f);
        OnSpellUnlocked?.Invoke(spellID);
    }

    public void SpellLockedIconAnimation(int spellID)
    {
        PlayBadge(spellLockedIcon, lockedRect,
            position: invSlotRect[spellID].anchoredPosition,
            startScale: 0.25f, endScale: 0.4f, scaleTime: 0.7f,
            peakAlpha: 1f, fadeInTime: 0.2f, fadeOutTime: 0.5f);
    }

    public void SelectingUnlockedAuraAnimation(int spellID)
    {
        PlayBadge(alreadyUnlockedIcon, alreadyUnlockedRect,
            position: invSlotRect[spellID].anchoredPosition - new Vector2(1.5f, -1),
            startScale: 0.5f, endScale: 0.7f, scaleTime: 0.35f,
            peakAlpha: 0.6f, fadeInTime: 0.1f, fadeOutTime: 0.3f);
        currentlySelectedSlotIndicator.DOMoveX(alreadyUnlockedRect.position.x, 0.1f).SetEase(Ease.OutExpo);
    }

    // shared pop-in/fade-out badge: appear at a slot, scale up, fade away, disable
    private static void PlayBadge(Image icon, RectTransform rect, Vector2 position,
        float startScale, float endScale, float scaleTime,
        float peakAlpha, float fadeInTime, float fadeOutTime)
    {
        icon.DOKill();
        rect.DOKill();
        icon.enabled = true;
        icon.color = new Color(1f, 1f, 1f, 0f);
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one * startScale;
        rect.DOScale(endScale, scaleTime);
        icon.DOFade(peakAlpha, fadeInTime)
            .OnComplete(() => icon.DOFade(0f, fadeOutTime)
            .OnComplete(() => icon.enabled = false));
    }

    private void PlayUnlockLight(int spellID)
    {
        spellUnlockedLight.DOKill();
        lightRect.DOKill();
        spellUnlockedLight.enabled = true;
        lightRect.anchoredPosition = invSlotRect[spellID].anchoredPosition;
        lightRect.localScale = Vector3.zero;
        lightRect.DOScaleY(1, 0.3f).SetEase(Ease.OutExpo);
        lightRect.DOScaleX(1, 1).SetEase(Ease.OutExpo);
        spellUnlockedLight.DOFade(1, 0.1f)
            .OnComplete(() => spellUnlockedLight.DOFade(0, 4)
            .OnComplete(() => spellUnlockedLight.enabled = false));
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (spellUnlocked == null) return;
        if ((invSlotRect != null && invSlotRect.Length != spellUnlocked.Length) ||
            (spellIcon != null && spellIcon.Length != spellUnlocked.Length) ||
            (lockedSpellTint != null && lockedSpellTint.Length != spellUnlocked.Length))
        {
            Debug.LogWarning("UnlockedSpells: per-spell arrays must all have the same length (one entry per spellID)", this);
        }
    }
#endif
}
