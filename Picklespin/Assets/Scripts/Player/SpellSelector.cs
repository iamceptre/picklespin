using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SpellSelector : MonoBehaviour
{
    // 0 means spell 1 
    // 1 means spell 2

    private Attack attack;
    [SerializeField] private EventReference spellLockedSoundEvent;
    private UnlockedSpells unlockedSpells;
    private InventoryBarSelectedSpell inventoryBarSelectedSpell;

    private int index = 0;

    private int spellIndexLimit = 1; //use the unclokedSpells[] list to work with selected index instead to allow for skipping

    //make scroll ignore locked spells and go to first unlocked one in a given direction
    //so when the 1 is unlocked, the 2 is locked, and the 3rd is unlocked, when the currently selected is 1 and player uses scroll up, it will go to spell 3

    //OR just make the spell index limit update to the highest (number) unlocked spell in UnlockedSpells

    private void Start()
    {
        attack = Attack.instance;
        unlockedSpells = UnlockedSpells.instance;
        inventoryBarSelectedSpell = InventoryBarSelectedSpell.instance;
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1)) //prevent changing during shooting and minigame
        {
            return;
        }
        HandleScroll();
        HandleKeys();
    }


    private void HandleScroll()
    {
        if (Input.GetAxisRaw("Mouse ScrollWheel") != 0f)
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");

            if (scroll > 0f) // Scrolling up
            {
                if (index == spellIndexLimit)
                {
                    index = 0;
                }
                else
                {
                    index = Mathf.Min(index + 1, spellIndexLimit);
                }
            }
            else if (scroll < 0f) // Scrolling down
            {
                if (index == 0)
                {
                    index = spellIndexLimit;
                }
                else
                {
                    index = Mathf.Max(index - 1, 0);
                }

            }

            ChooseSpell(index);
        }
    }

    private void HandleKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSpell(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetSpell(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetSpell(2);
        }
    }


    private void SetSpell(int newIndex)
    {
        if (index != newIndex)
        {
            index = newIndex;
            ChooseSpell(index);
        }
    }

    private void ChooseSpell(int pressedNumber)
    {
        if (!Input.GetKey(KeyCode.Mouse0))
        {
            inventoryBarSelectedSpell.NumberBump(pressedNumber);

            if (unlockedSpells.spellUnlocked[pressedNumber] == true)
            {
                unlockedSpells.SelectingUnlockedAuraAnimation(pressedNumber);
                attack.SelectSpell(pressedNumber);
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
