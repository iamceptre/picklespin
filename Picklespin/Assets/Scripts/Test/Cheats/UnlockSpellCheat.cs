using UnityEngine;

public class UnlockSpellCheat : MonoBehaviour
{

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private Ammo ammo;

    private CheatActivatedFeedback cheatActivatedFeedback;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
    }

    void Update()
    {

        if(Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                unlockedSpells.UnlockASpell(2);
                cheatActivatedFeedback.Do("unlock light spell");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                unlockedSpells.UnlockASpell(1);
                cheatActivatedFeedback.Do("unlock fireball");
            }


            if (Input.GetKeyDown(KeyCode.M))
            {
                ammo.GiveManaToPlayer(ammo.maxAmmo - ammo.ammo, false);
                cheatActivatedFeedback.Do("full mana");
            }
        }

    }
}
