using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryBarSelectedSpell : MonoBehaviour
{
    public static InventoryBarSelectedSpell instance;
    [SerializeField] Image[] invSlot;
    [SerializeField] Image[] invSlotSpellIcon;
    [SerializeField] RectTransform[] invNumbersRect;
    [SerializeField] Attack attack;
    float invNumberStartPos;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    void Start()
    {
        SelectionChanged(attack ? attack.selectedSpell : SpellId.Netherlight);
        invNumberStartPos = invNumbersRect[0].localPosition.y;
    }

    static readonly Color dimmedIconColor = GameColors.Dimmed;

    public void SelectionChanged(SpellId spell)
    {
        for (int i = 0; i < invSlot.Length; i++)
        {
            bool selected = i == (int)spell;
            invSlot[i].color = selected ? GameColors.Neutral : GameColors.Ghost;
            invSlotSpellIcon[i].color = selected ? GameColors.Neutral : dimmedIconColor;
        }
    }

    public void NumberBump(SpellId spell)
    {
        RectTransform number = invNumbersRect[(int)spell];
        number.DOKill();
        number.localPosition = new Vector2(number.localPosition.x, invNumberStartPos);
        number.DOLocalMoveY(number.localPosition.y - 10, 0.1f).SetLoops(2, LoopType.Yoyo);
    }
}
