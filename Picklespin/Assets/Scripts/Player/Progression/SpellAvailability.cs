using System;
using UnityEngine;

public static class SpellAvailability
{
    public static event Action Changed;

    private static readonly int[] pickupsOnMap = new int[Enum.GetValues(typeof(SpellId)).Length];

    public static int SpellCount => pickupsOnMap.Length;

    public static bool IsObtainable(SpellId spell)
    {
        UnlockedSpells unlocked = UnlockedSpells.instance;
        if (unlocked && unlocked.IsUnlocked(spell)) return true;
        if (pickupsOnMap[(int)spell] > 0) return true;
        return GrantedByClass(spell);
    }

    private static bool GrantedByClass(SpellId spell)
    {
        if (PlayerClasses.LockedSpell == spell) return true;
        return spell == SpellId.Light && PlayerClasses.LightSpellConverts;
    }

    public static void PickupPlaced(SpellId spell)
    {
        pickupsOnMap[(int)spell]++;
        Changed?.Invoke();
    }

    public static void PickupRemoved(SpellId spell)
    {
        int slot = (int)spell;
        if (pickupsOnMap[slot] > 0) pickupsOnMap[slot]--;
        Changed?.Invoke();
    }

    public static void NotifyChanged() => Changed?.Invoke();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Array.Clear(pickupsOnMap, 0, pickupsOnMap.Length);
        Changed = null;
        PlayerClasses.Changed -= NotifyChanged;
        PlayerClasses.Changed += NotifyChanged;
    }
}
