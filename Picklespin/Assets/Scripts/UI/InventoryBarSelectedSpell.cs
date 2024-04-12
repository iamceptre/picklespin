using UnityEngine;
using UnityEngine.UI;

public class InventoryBarSelectedSpell : MonoBehaviour
{
    [SerializeField] private Image[] invSlot;
    [SerializeField] private Image[] invSlotSpellIcon;
    [SerializeField] private Attack attack;

    private void Start()
    {
        SelectionChanged();
    }


    public void SelectionChanged()
    {

        for (int i = 0; i < invSlot.Length; i++)
        {
            invSlot[i].color = Color.gray;
            invSlotSpellIcon[i].color = Color.gray;
        }

        invSlot[attack.selectedBullet].color = Color.white;
        invSlotSpellIcon[attack.selectedBullet].color = Color.white;
    }

}
