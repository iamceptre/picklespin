using UnityEngine;

public class SpellPickupable : MonoBehaviour
{
    [SerializeField] private int spellID;
    private UnlockedSpells unlockedSpells;


    private void Awake()
    {
        unlockedSpells = GameObject.FindWithTag("Player").GetComponent<UnlockedSpells>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            unlockedSpells.UnlockASpell(spellID);
            Destroy(gameObject);
        }

    }
}
