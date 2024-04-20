using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class InventoryBarSelectedSpell : MonoBehaviour
{
    [SerializeField] private Image[] invSlot;
    [SerializeField] private Image[] invSlotSpellIcon;
    [SerializeField] private RectTransform[] invNumbersRect;
    [SerializeField] private Attack attack;

    private float invNumberStartPos;

    private void Start()
    {
        SelectionChanged();
        invNumberStartPos = invNumbersRect[0].localPosition.y;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            NumberBump(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            NumberBump(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            NumberBump(2);
        }
    }


    public void SelectionChanged() //its called from attack script
    {

        for (int i = 0; i < invSlot.Length; i++)
        {
            invSlot[i].color = Color.gray;
            invSlotSpellIcon[i].color = new Color(0.35f, 0.35f, 0.35f);
        }

        invSlot[attack.selectedBullet].color = Color.white;
        invSlotSpellIcon[attack.selectedBullet].color = Color.white;
    }

    private void NumberBump(int spellID)
    {
        invNumbersRect[spellID].DOKill();
        invNumbersRect[spellID].localPosition = new Vector2(invNumbersRect[spellID].localPosition.x, invNumberStartPos);
        invNumbersRect[spellID].DOLocalMoveY(invNumbersRect[spellID].localPosition.y - 10, 0.1f).SetLoops(2,LoopType.Yoyo);
    }

}
