using UnityEngine;

// Lives on each enemy: registers it with the global counter when it becomes active.
// Counting is idempotent so pooled reuse, the death event (deCountMe) and pool
// release (OnDisable) can never double-count.
public class EnemyCounter_PerUnitComponent : MonoBehaviour
{
    private bool counted;

    private void OnEnable()
    {
        if (counted || EnemyCounter.instance == null) return;
        counted = true;
        EnemyCounter.instance.Register();
    }

    private void Start()
    {
        // scene-placed enemies may enable before the counter's Awake
        if (!counted && EnemyCounter.instance != null)
        {
            counted = true;
            EnemyCounter.instance.Register();
        }
    }

    public void deCountMe()
    {
        Deregister();
    }

    private void OnDisable()
    {
        Deregister();
    }

    private void Deregister()
    {
        if (!counted) return;
        counted = false;
        if (EnemyCounter.instance != null) EnemyCounter.instance.Deregister();
    }
}
