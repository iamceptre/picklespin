using UnityEngine;

public class UnlockSpellCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private Ammo ammo;

    private CheatActivatedFeedback cheatActivatedFeedback;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
        DevLog.Info($"{nameof(UnlockSpellCheat)} armed: Up Arrow + 2 unlocks {SpellId.Fireball}, " +
                    $"Up Arrow + 3 unlocks {SpellId.Light}, Up Arrow + M refills magicka", this);
    }

    void Update()
    {

        if(InputCompat.GetKey(KeyCode.UpArrow))
        {
            if (InputCompat.GetKeyDown(KeyCode.Alpha3))
            {
                unlockedSpells.UnlockASpell(SpellId.Light);
                cheatActivatedFeedback.Do("unlock light spell");
            }

            if (InputCompat.GetKeyDown(KeyCode.Alpha2))
            {
                unlockedSpells.UnlockASpell(SpellId.Fireball);
                cheatActivatedFeedback.Do("unlock fireball");
            }


            if (InputCompat.GetKeyDown(KeyCode.M))
            {
                ammo.GiveManaToPlayer(ammo.maxAmmo - ammo.ammo, false);
                cheatActivatedFeedback.Do("full mana");
            }
        }

    }
#endif
}
