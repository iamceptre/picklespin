using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryBarSelectedSpell : MonoBehaviour
{

    public static InventoryBarSelectedSpell instance;

    [SerializeField] private Image[] invSlot;
    [SerializeField] private Image[] invSlotSpellIcon;
    [SerializeField] private RectTransform[] invNumbersRect;
    [SerializeField] private Attack attack;

    private float invNumberStartPos;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        SelectionChanged(0);
        invNumberStartPos = invNumbersRect[0].localPosition.y;
    }


    public void SelectionChanged(int spellID) 
    {
        for (int i = 0; i < invSlot.Length; i++)
        {
            invSlot[i].color = Color.gray;
            invSlotSpellIcon[i].color = new Color(0.35f, 0.35f, 0.35f);
        }

        invSlot[spellID].color = Color.white;
        invSlotSpellIcon[spellID].color = Color.white;
    }

    public void NumberBump(int spellID)
    {
        invNumbersRect[spellID].DOKill();
        invNumbersRect[spellID].localPosition = new Vector2(invNumbersRect[spellID].localPosition.x, invNumberStartPos);
        invNumbersRect[spellID].DOLocalMoveY(invNumbersRect[spellID].localPosition.y - 10, 0.1f).SetLoops(2,LoopType.Yoyo);
    }

}
