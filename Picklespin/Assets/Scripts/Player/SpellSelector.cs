using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

// Spell switching. Scroll cycles through unlocked spells (locked ones are skipped),
// digit keys jump directly and give locked feedback. The number of spells derives
// from the spell arrays — adding a spell needs no changes here.
public class SpellSelector : MonoBehaviour
{
    [SerializeField] EventReference spellLockedSoundEvent;
    [SerializeField] InputActionReference scrollAction;
    [SerializeField] InputActionReference attackAction;
    [SerializeField] InputActionReference healAction;

    private Attack attack;
    private UnlockedSpells unlockedSpells;
    private InventoryBarSelectedSpell inventoryBar;
    private int index;
    private int spellCount;
    private float lastScrollValue;

    void Start()
    {
        attack = Attack.instance;
        unlockedSpells = UnlockedSpells.instance;
        inventoryBar = InventoryBarSelectedSpell.instance;
        spellCount = Mathf.Min(unlockedSpells.SpellCount, attack.bulletPrefab.Length);
    }

    void Update()
    {
        if (PlayerClasses.LockedSpellIndex >= 0) return; // Umbral has nothing to switch to
        if (attackAction.action.IsPressed() || healAction.action.IsPressed()) return;
        HandleScroll();
        HandleDigitKeys();
    }

    void HandleScroll()
    {
        float scroll = scrollAction.action.ReadValue<float>();
        if (scroll >= 0.5f && lastScrollValue < 0.5f) SelectNextUnlocked(+1);
        else if (scroll <= -0.5f && lastScrollValue > -0.5f) SelectNextUnlocked(-1);
        lastScrollValue = scroll;
    }

    void HandleDigitKeys()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        for (int i = 0; i < spellCount; i++)
        {
            if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
            {
                ChooseSpell(i);
                return;
            }
        }
    }

    void SelectNextUnlocked(int direction)
    {
        for (int step = 1; step < spellCount; step++)
        {
            int candidate = ((index + direction * step) % spellCount + spellCount) % spellCount;
            if (unlockedSpells.IsUnlocked(candidate))
            {
                ChooseSpell(candidate);
                return;
            }
        }
    }

    void ChooseSpell(int newIndex)
    {
        index = newIndex;
        inventoryBar.NumberBump(newIndex);

        if (unlockedSpells.IsUnlocked(newIndex))
        {
            unlockedSpells.SelectingUnlockedAuraAnimation(newIndex);
            attack.SelectSpell(newIndex);
            inventoryBar.SelectionChanged(newIndex);
        }
        else
        {
            RuntimeManager.PlayOneShot(spellLockedSoundEvent);
            unlockedSpells.SpellLockedIconAnimation(newIndex);
        }
    }
}
