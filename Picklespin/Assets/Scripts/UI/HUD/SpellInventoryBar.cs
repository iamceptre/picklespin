using DG.Tweening;
using UnityEngine;

public class SpellInventoryBar : MonoBehaviour
{
    public static SpellInventoryBar instance { get; private set; }

    [Header("Slots")]
    [SerializeField, Tooltip("disabled slot object cloned once per spell at startup - the clones live under its parent")]
    private SpellInventorySlot slotTemplate;
    [SerializeField, Tooltip("icon sprite per spell (index = SpellId, order must match the enum)")]
    private Sprite[] spellIcons;
    [SerializeField, Tooltip("distance between neighbouring slot centres - the row re-centres itself around the template's x")]
    private float slotSpacing = 42.4f;
    [SerializeField] private float layoutTweenTime = 0.3f;

    [SerializeField, Tooltip("marker under the selected slot - must be a child of the slots' parent")]
    private RectTransform currentlySelectedSlotIndicator;

    public int VisibleCount => visibleCount;

    private SpellInventorySlot[] slots;
    private bool[] slotShown;
    private int[] visibleSpell;
    private bool[] obtainable;
    private int visibleCount;
    private SpellId selectedSpell;

    private float rowY;
    private Vector3 slotBaseScale;
    private UnlockedSpells unlockedSpells;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;

        RectTransform templateRect = (RectTransform)slotTemplate.transform;
        rowY = templateRect.anchoredPosition.y;
        slotBaseScale = templateRect.localScale;
        slotTemplate.gameObject.SetActive(false);

        slots = new SpellInventorySlot[spellIcons.Length];
        slotShown = new bool[spellIcons.Length];
        visibleSpell = new int[spellIcons.Length];
        obtainable = new bool[spellIcons.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            SpellInventorySlot slot = Instantiate(slotTemplate, templateRect.parent);
            slot.gameObject.SetActive(true);
            slot.Assign(spellIcons[i]);
            slot.Group.alpha = 0f;
            slots[i] = slot;
        }
    }

    private void OnEnable() => SpellAvailability.Changed += Refresh;

    private void OnDisable()
    {
        SpellAvailability.Changed -= Refresh;
        if (unlockedSpells) unlockedSpells.Unlocked -= OnSpellUnlocked;
    }

    private void Start()
    {
        unlockedSpells = UnlockedSpells.instance;
        unlockedSpells.Unlocked += OnSpellUnlocked;
        selectedSpell = Attack.instance ? Attack.instance.selectedSpell : SpellId.Netherlight;
        Refresh();
    }

    public SpellId SpellAt(int visibleSlot) => (SpellId)visibleSpell[visibleSlot];

    public int SlotOf(SpellId spell)
    {
        int target = (int)spell;
        for (int v = 0; v < visibleCount; v++)
        {
            if (visibleSpell[v] == target) return v;
        }
        return -1;
    }

    public void NumberBump(SpellId spell) => slots[(int)spell].NumberBump();

    public void Select(SpellId spell)
    {
        selectedSpell = spell;
        for (int v = 0; v < visibleCount; v++)
        {
            int i = visibleSpell[v];
            slots[i].ApplyState(unlockedSpells.IsUnlocked((SpellId)i), i == (int)spell);
        }
        slots[(int)spell].PlaySelectedAura();
        MoveIndicator(0.1f);
    }

    public void Deny(SpellId spell) => slots[(int)spell].PlayDeny();

    private void Refresh()
    {
        if (slots == null || !unlockedSpells) return;

        visibleCount = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            obtainable[i] = SpellAvailability.IsObtainable((SpellId)i);
            if (obtainable[i]) visibleSpell[visibleCount++] = i;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (!obtainable[i]) HideSlot(i);
        }

        for (int v = 0; v < visibleCount; v++)
        {
            int i = visibleSpell[v];
            SpellInventorySlot slot = slots[i];
            slot.SetNumber(v + 1);
            slot.ApplyState(unlockedSpells.IsUnlocked((SpellId)i), i == (int)selectedSpell);

            Vector2 target = new(SlotX(v), rowY);
            if (slotShown[i]) MoveSlot(slot, target);
            else ShowSlot(i, target);
        }

        MoveIndicator(layoutTweenTime);
    }

    private float SlotX(int visibleSlot) => ((1 - visibleCount) * 0.5f + visibleSlot) * slotSpacing;

    private void ShowSlot(int spellIndex, Vector2 target)
    {
        slotShown[spellIndex] = true;
        SpellInventorySlot slot = slots[spellIndex];
        slot.Rect.DOKill();
        slot.Rect.anchoredPosition = target;
        slot.Rect.localScale = slotBaseScale * 0.5f;
        slot.Rect.DOScale(slotBaseScale, layoutTweenTime).SetEase(Ease.OutBack);
        slot.Group.DOKill();
        slot.Group.DOFade(1f, layoutTweenTime);
    }

    private void MoveSlot(SpellInventorySlot slot, Vector2 target)
    {
        slot.Rect.DOKill();
        slot.Rect.localScale = slotBaseScale;
        slot.Rect.DOAnchorPos(target, layoutTweenTime).SetEase(Ease.OutExpo);
    }

    private void HideSlot(int spellIndex)
    {
        if (!slotShown[spellIndex]) return;
        slotShown[spellIndex] = false;
        SpellInventorySlot slot = slots[spellIndex];
        slot.Rect.DOKill();
        slot.Group.DOKill();
        slot.Group.DOFade(0f, layoutTweenTime);
    }

    private void MoveIndicator(float duration)
    {
        if (!currentlySelectedSlotIndicator) return;
        int v = SlotOf(selectedSpell);
        if (v < 0) return;
        currentlySelectedSlotIndicator.DOKill();
        currentlySelectedSlotIndicator.DOAnchorPosX(SlotX(v), duration).SetEase(Ease.OutExpo);
    }

    private void OnSpellUnlocked(SpellId spell) => slots[(int)spell].PlayUnlockFX();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spellIcons != null && spellIcons.Length != 0 && spellIcons.Length != SpellAvailability.SpellCount)
        {
            DevLog.Warn("SpellInventoryBar: spellIcons must hold one sprite per SpellId", this);
        }
    }
#endif
}
