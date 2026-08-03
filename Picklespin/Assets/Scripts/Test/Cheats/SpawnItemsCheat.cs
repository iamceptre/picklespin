using UnityEngine;

public class SpawnItemsCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private CheatActivatedFeedback cheatActivatedFeedback;
    private SpellSpawner spellSpawner;
    private PickableBonusesSpawner pickableBonusesSpawner;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
        spellSpawner = SpellSpawner.instance;
        pickableBonusesSpawner = PickableBonusesSpawner.instance;
        DevLog.Info($"{nameof(SpawnItemsCheat)} armed: Up Arrow + L spawns a spell pickup, Up Arrow + B spawns 8 bonuses", this);
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
                pickableBonusesSpawner.SpawnBonuses(8);
                cheatActivatedFeedback.Do("spawn bonuses");
            }

        }

    }
#endif
}
