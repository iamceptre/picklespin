using UnityEngine;
using FMODUnity;

public class UnlockSpellCheat : MonoBehaviour
{

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private EventReference cheatCodeSound;
    
    void Update()
    {

        if(Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                unlockedSpells.spellUnlocked[2] = true;
                RuntimeManager.PlayOneShot(cheatCodeSound);
            }
        }
        
    }
}
