using FMODUnity;
using UnityEngine;

public class SpellSelector : MonoBehaviour
{
    //0 means spell 1 
    //1 means spell 2

    private Attack attack;
    [SerializeField] private EventReference spellLockedSoundEvent;
    private UnlockedSpells unlockedSpells;
    private int currentlySelectedSpell;
    private InventoryBarSelectedSpell inventoryBarSelectedSpell;

    private void Start()
    {
        attack = Attack.instance;
        unlockedSpells = UnlockedSpells.instance;
        inventoryBarSelectedSpell = InventoryBarSelectedSpell.instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChooseSpell(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChooseSpell(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChooseSpell(2);
        }
    }



    private void ChooseSpell(int pressedNumber)
    {

        inventoryBarSelectedSpell.NumberBump(pressedNumber);

        if (pressedNumber != currentlySelectedSpell) { //prevents mindless spamming 

            if (unlockedSpells.spellUnlocked[pressedNumber] == true)
            {
                unlockedSpells.SelectingUnlockedAuraAnimation(pressedNumber);
                attack.SelectSpell(pressedNumber);
                currentlySelectedSpell = pressedNumber;
                inventoryBarSelectedSpell.SelectionChanged(pressedNumber);
            }
            else
            {
                RuntimeManager.PlayOneShot(spellLockedSoundEvent);
                unlockedSpells.spellLockedIconAnimation(pressedNumber);
            }
        }

    }

   
}
