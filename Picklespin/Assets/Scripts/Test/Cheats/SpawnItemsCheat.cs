using UnityEngine;

public class SpawnItemsCheat : MonoBehaviour
{

    private CheatActivatedFeedback cheatActivatedFeedback;
    private SpellSpawner spellSpawner;
    private PickupableBonusesSpawner pickupableBonusesSpawner;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
        spellSpawner = SpellSpawner.instance;
        pickupableBonusesSpawner = PickupableBonusesSpawner.instance;
    }

    void Update()
    {

        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                spellSpawner.SpawnSpellsLo(1);
                cheatActivatedFeedback.Do("spawn low spells");
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                pickupableBonusesSpawner.SpawnBonuses(8);
                cheatActivatedFeedback.Do("spawn bonuses");
            }

        }

    }
}
