using UnityEngine;
using FMODUnity;

public class UnlockSpellCheat : MonoBehaviour
{

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private Ammo ammo;

    [SerializeField] private EventReference cheatCodeSound;
    
    void Update()
    {

        if(Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                unlockedSpells.UnlockASpell(2);
                RuntimeManager.PlayOneShot(cheatCodeSound);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                unlockedSpells.UnlockASpell(1);
                RuntimeManager.PlayOneShot(cheatCodeSound);
            }


            if (Input.GetKeyDown(KeyCode.M))
            {
                ammo.ammo = ammo.maxAmmo;
                RuntimeManager.PlayOneShot(cheatCodeSound);
            }
        }

    }
}
