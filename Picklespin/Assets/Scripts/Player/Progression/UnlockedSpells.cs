using System;
using FMODUnity;
using UnityEngine;

public class UnlockedSpells : MonoBehaviour
{
    public static UnlockedSpells instance { get; private set; }

    [Header("State (index = SpellId)")]
    public bool[] spellUnlocked;

    public event Action<SpellId> Unlocked;

    public int SpellCount => spellUnlocked.Length;

    private const int DuplicateUnlockManaRefund = 50;
    private const string DuplicateUnlockSound = "event:/ITEMS/POTIONS/POTION_PICKUP_BASE_LAYER";

    private Ammo ammo;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    private void Start()
    {
        ammo = Ammo.instance;
    }

    public bool IsUnlocked(SpellId spell)
    {
        int slot = (int)spell;
        return slot < spellUnlocked.Length && spellUnlocked[slot];
    }

    public void UnlockASpell(SpellId spell)
    {
        int slot = (int)spell;
        if (slot >= spellUnlocked.Length)
        {
            DevLog.Warn($"{nameof(UnlockedSpells)}: no unlock entry for {spell} - size spellUnlocked to one per SpellId", this);
            return;
        }

        if (spellUnlocked[slot])
        {
            ammo.GiveManaToPlayer(DuplicateUnlockManaRefund, false);
            RuntimeManager.PlayOneShot(DuplicateUnlockSound);
            return;
        }

        spellUnlocked[slot] = true;
        SpellAvailability.NotifyChanged();
        Unlocked?.Invoke(spell);
    }
}
