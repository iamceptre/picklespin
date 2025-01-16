using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSelector : MonoBehaviour
{
    private Attack attack;
    [SerializeField] EventReference spellLockedSoundEvent;
    private UnlockedSpells unlockedSpells;
    private InventoryBarSelectedSpell inventoryBarSelectedSpell;
    private int index;
    private readonly int spellIndexLimit = 1;
    [SerializeField] InputActionReference scrollAction;
    [SerializeField] InputActionReference attackAction;
    [SerializeField] InputActionReference healAction;
    float lastScrollValue;

    //void OnEnable()
    //{
    //    scrollAction.action.Enable();
    //    attackAction.action.Enable();
    //    healAction.action.Enable();
    //}

    //void OnDisable()
    //{
    //    scrollAction.action.Disable();
    //    attackAction.action.Disable();
    //    healAction.action.Disable();
    //}

    void Start()
    {
        attack = Attack.instance;
        unlockedSpells = UnlockedSpells.instance;
        inventoryBarSelectedSpell = InventoryBarSelectedSpell.instance;
        lastScrollValue = 0f;
    }

    void Update()
    {
        if (attackAction.action.IsPressed() || healAction.action.IsPressed()) return;
        HandleScroll();
        HandleKeys();
    }

    void HandleScroll()
    {
        float currentScroll = scrollAction.action.ReadValue<float>();
        if (currentScroll >= 0.5f && lastScrollValue < 0.5f)
        {
            if (index == spellIndexLimit) index = 0; else index = Mathf.Min(index + 1, spellIndexLimit);
            ChooseSpell(index);
        }
        else if (currentScroll <= -0.5f && lastScrollValue > -0.5f)
        {
            if (index == 0) index = spellIndexLimit; else index = Mathf.Max(index - 1, 0);
            ChooseSpell(index);
        }
        lastScrollValue = currentScroll;
    }

    void HandleKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChooseSpell(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChooseSpell(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChooseSpell(2);
    }

    void ChooseSpell(int newIndex)
    {
        index = newIndex;
        if (!Input.GetKey(KeyCode.Mouse0))
        {
            inventoryBarSelectedSpell.NumberBump(newIndex);
            if (unlockedSpells.spellUnlocked[newIndex])
            {
                unlockedSpells.SelectingUnlockedAuraAnimation(newIndex);
                attack.SelectSpell(newIndex);
                inventoryBarSelectedSpell.SelectionChanged(newIndex);
            }
            else
            {
                RuntimeManager.PlayOneShot(spellLockedSoundEvent);
                unlockedSpells.SpellLockedIconAnimation(newIndex);
            }
        }
    }
}
