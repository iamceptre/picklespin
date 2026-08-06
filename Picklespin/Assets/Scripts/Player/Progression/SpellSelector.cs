using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellSelector : MonoBehaviour
{
    [SerializeField] private EventReference spellLockedSoundEvent;
    [SerializeField] private InputActionReference scrollAction;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference healAction;

    private Attack attack;
    private UnlockedSpells unlockedSpells;
    private SpellInventoryBar bar;
    private float lastScrollValue;

    private void Start()
    {
        attack = Attack.instance;
        unlockedSpells = UnlockedSpells.instance;
        bar = SpellInventoryBar.instance;
    }

    private void Update()
    {
        if (PlayerClasses.LockedSpell.HasValue) return;
        if (attackAction.action.IsPressed() || healAction.action.IsPressed()) return;
        HandleScroll();
        HandleDigitKeys();
    }

    private void HandleScroll()
    {
        float scroll = scrollAction.action.ReadValue<float>();
        if (scroll >= 0.5f && lastScrollValue < 0.5f) SelectNextUnlocked(+1);
        else if (scroll <= -0.5f && lastScrollValue > -0.5f) SelectNextUnlocked(-1);
        lastScrollValue = scroll;
    }

    private void HandleDigitKeys()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        int slotCount = bar.VisibleCount;
        for (int i = 0; i < slotCount; i++)
        {
            if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
            {
                ChooseSlot(i);
                return;
            }
        }
    }

    private void SelectNextUnlocked(int direction)
    {
        int slotCount = bar.VisibleCount;
        if (slotCount == 0) return;
        int current = bar.SlotOf(attack.selectedSpell);
        if (current < 0) current = 0;
        for (int step = 1; step < slotCount; step++)
        {
            int candidate = ((current + direction * step) % slotCount + slotCount) % slotCount;
            if (unlockedSpells.IsUnlocked(bar.SpellAt(candidate)))
            {
                ChooseSlot(candidate);
                return;
            }
        }
    }

    private void ChooseSlot(int visibleSlot)
    {
        SpellId spell = bar.SpellAt(visibleSlot);
        bar.NumberBump(spell);

        if (unlockedSpells.IsUnlocked(spell))
        {
            if (attack.SelectSpell(spell)) bar.Select(spell);
        }
        else
        {
            RuntimeManager.PlayOneShot(spellLockedSoundEvent);
            bar.Deny(spell);
        }
    }
}
