using UnityEngine;
using UnityEngine.Pool;

// Mercy drops. When a pool runs dry, a few potions for it are put out on the map -
// always at a spawn point the player cannot see just then, so they are come across
// rather than watched appearing. Spawn points and their taken flags are the arena's
// own (PickupableBonusesSpawner), so a drop can never land on a bonus.
public class LowResourcePotionDrop : MonoBehaviour
{
    private const int Health = 0;
    private const int Stamina = 1;
    private const int Magicka = 2;
    private const int PoolCount = 3;

    [Header("One potion prefab per resource")]
    [SerializeField] private PoolSpawnableObject healthPotion;
    [SerializeField] private PoolSpawnableObject staminaPotion;
    [SerializeField] private PoolSpawnableObject magickaPotion;

    [Header("Drop")]
    [SerializeField, Range(0f, 1f), Tooltip("share of the pool below which the drop is made")]
    private float threshold = 0.1f;
    [SerializeField] private int potionsPerDrop = 3;
    [SerializeField, Tooltip("seconds between checks")]
    private float checkInterval = 0.5f;
    [SerializeField, Tooltip("seconds after a drop before any pool is looked at again")]
    private float dropCooldown = 4f;

    [Header("Hiding the spawn")]
    [SerializeField, Tooltip("auto-found from Camera.main if left empty")]
    private Camera playerCamera;
    [SerializeField, Tooltip("geometry solid enough to hide a spawn point standing in view")]
    private LayerMask occluders = ~0;
    [SerializeField, Tooltip("viewport margin still counted as seen, so a small turn cannot reveal a spawn")]
    private float viewMargin = 0.1f;

    private PickupableBonusesSpawner spawner;
    private PoolSpawnableObject[] prefabs;
    private readonly ObjectPool<PoolSpawnableObject>[] pools = new ObjectPool<PoolSpawnableObject>[PoolCount];
    private readonly Vector3 buriedPosition = new(0f, -50f, 0f);
    private float nextCheckTime;

    private void Start()
    {
        spawner = PickupableBonusesSpawner.instance;
        if (!playerCamera) playerCamera = Camera.main;

        prefabs = new[] { healthPotion, staminaPotion, magickaPotion };
        for (int i = 0; i < PoolCount; i++)
        {
            if (!prefabs[i]) continue;
            int pool = i;
            pools[i] = new ObjectPool<PoolSpawnableObject>(
                () => Create(pool),
                potion => potion.gameObject.SetActive(true),
                potion => { potion.gameObject.SetActive(false); potion.transform.position = buriedPosition; },
                potion => Destroy(potion.gameObject),
                false, potionsPerDrop, potionsPerDrop * 2);
        }

        InvokeRepeating(nameof(CheckPools), checkInterval, checkInterval);
    }

    // A class can fold one pool into another - Umbral spends the black bar for all
    // three - and one empty bar must not drop three kinds of potion at once.
    private void CheckPools()
    {
        if (!spawner || Time.time < nextCheckTime) return;

        if (!PlayerClasses.MagickaIsHealth && TryDrop(Health)) return;
        if (!PlayerClasses.StaminaSharesMagicka && TryDrop(Stamina)) return;
        TryDrop(Magicka);
    }

    private bool TryDrop(int pool)
    {
        if (Fraction(pool) >= threshold) return false;

        nextCheckTime = Time.time + dropCooldown;
        Drop(pool);
        return true;
    }

    private float Fraction(int pool) => pool switch
    {
        Health => PlayerHP.Instance && PlayerHP.Instance.maxHp > 0
            ? (float)PlayerHP.Instance.hp / PlayerHP.Instance.maxHp
            : 1f,
        Stamina => PlayerMovement.Instance ? PlayerMovement.Instance.StaminaFraction : 1f,
        _ => Ammo.instance ? Ammo.instance.Fraction : 1f
    };

    // the arena's own scatter: same free-point pick, same stagger, same occupancy -
    // only the pool differs, and the points have to be ones nobody is looking at
    private void Drop(int pool)
    {
        if (pools[pool] == null) return;
        spawner.ScatterSpawn(pools[pool].Get, potionsPerDrop, IsOutOfSight);
    }

    private bool IsOutOfSight(Vector3 point)
    {
        if (!playerCamera) return true;

        Vector3 viewport = playerCamera.WorldToViewportPoint(point);
        bool inFrustum = viewport.z > 0f
                         && viewport.x > -viewMargin && viewport.x < 1f + viewMargin
                         && viewport.y > -viewMargin && viewport.y < 1f + viewMargin;
        if (!inFrustum) return true;

        // in the frustum but behind something still counts as unseen. The ray stops
        // short of the point, or whatever the potion would rest on reads as cover.
        Vector3 eye = playerCamera.transform.position;
        return Physics.Linecast(eye, Vector3.MoveTowards(point, eye, 0.3f), occluders, QueryTriggerInteraction.Ignore);
    }

    private PoolSpawnableObject Create(int pool)
    {
        PoolSpawnableObject potion = Instantiate(prefabs[pool], buriedPosition, Quaternion.identity);
        potion.SetPool(pools[pool]);
        return potion;
    }
}
