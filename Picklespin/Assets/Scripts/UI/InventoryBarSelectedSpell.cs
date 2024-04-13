using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryBarSelectedSpell : MonoBehaviour
{
    [SerializeField] private Image[] invSlot;
    [SerializeField] private Image[] invSlotSpellIcon;
    [SerializeField] private RectTransform[] invNumbers;
    [SerializeField] private Attack attack;

    private float invNumberStartPos;

    private void Start()
    {
        SelectionChanged();
        invNumberStartPos = invNumbers[0].localPosition.y;
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


    public void SelectionChanged()
    {

        for (int i = 0; i < invSlot.Length; i++)
        {
            invSlot[i].color = Color.gray;
            invSlotSpellIcon[i].color = Color.black;
        }

        invSlot[attack.selectedBullet].color = Color.white;
        invSlotSpellIcon[attack.selectedBullet].color = Color.white;
    }

    public void NumberBump(int spellID)
    {
        invNumbers[spellID].DOKill();
        invNumbers[spellID].localPosition = new Vector2(invNumbers[spellID].localPosition.x, invNumberStartPos);
        invNumbers[spellID].DOLocalMoveY(invNumbers[spellID].localPosition.y - 10, 0.1f).SetLoops(2,LoopType.Yoyo);
    }

}
