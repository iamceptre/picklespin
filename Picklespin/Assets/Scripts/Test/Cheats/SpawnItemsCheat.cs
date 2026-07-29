using UnityEngine;

public class SpawnItemsCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

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

        if (InputCompat.GetKey(KeyCode.UpArrow))
        {
            if (InputCompat.GetKeyDown(KeyCode.L))
            {
                spellSpawner.SpawnSpellsLo(1);
                cheatActivatedFeedback.Do("spawn low spells");
            }

            if (InputCompat.GetKeyDown(KeyCode.B))
            {
                pickupableBonusesSpawner.SpawnBonuses(8);
                cheatActivatedFeedback.Do("spawn bonuses");
            }

        }

    }
#endif
}
