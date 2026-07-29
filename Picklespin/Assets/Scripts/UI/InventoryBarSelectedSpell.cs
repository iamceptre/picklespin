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
        SelectionChanged(0);
        invNumberStartPos = invNumbersRect[0].localPosition.y;
    }

    static readonly Color dimmedIconColor = new(0.35f, 0.35f, 0.35f);

    public void SelectionChanged(int spellID)
    {
        for (int i = 0; i < invSlot.Length; i++)
        {
            bool selected = i == spellID;
            invSlot[i].color = selected ? Color.white : Color.gray;
            invSlotSpellIcon[i].color = selected ? Color.white : dimmedIconColor;
        }
    }

    public void NumberBump(int spellID)
    {
        invNumbersRect[spellID].DOKill();
        invNumbersRect[spellID].localPosition = new Vector2(invNumbersRect[spellID].localPosition.x, invNumberStartPos);
        invNumbersRect[spellID].DOLocalMoveY(invNumbersRect[spellID].localPosition.y - 10, 0.1f).SetLoops(2, LoopType.Yoyo);
    }
}
