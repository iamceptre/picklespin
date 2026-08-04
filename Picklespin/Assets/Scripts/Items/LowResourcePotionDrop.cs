using UnityEngine;
using UnityEngine.Pool;

public class LowResourcePotionDrop : MonoBehaviour
{
    private const int Health = 0;
    private const int Stamina = 1;
    private const int Magicka = 2;
    private const int Umbral = 3;
    private const int PoolCount = 4;

    [Header("One potion prefab per resource")]
    [SerializeField] private PoolSpawnableObject healthPotion;
    [SerializeField] private PoolSpawnableObject staminaPotion;
    [SerializeField] private PoolSpawnableObject magickaPotion;
    [SerializeField, Tooltip("dropped instead of the three above while the Umbral class is held")]
    private PoolSpawnableObject umbralPotion;

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

    private PickableBonusesSpawner spawner;
    private RoundSystem roundSystem;
    private PoolSpawnableObject[] prefabs;
    private readonly ObjectPool<PoolSpawnableObject>[] pools = new ObjectPool<PoolSpawnableObject>[PoolCount];
    private readonly Vector3 buriedPosition = new(0f, -50f, 0f);
    private float nextCheckTime;

    private void Start()
    {
        spawner = PickableBonusesSpawner.instance;
        roundSystem = RoundSystem.instance;
        if (!playerCamera) playerCamera = Camera.main;

        prefabs = new[] { healthPotion, staminaPotion, magickaPotion, umbralPotion };
        for (int i = 0; i < PoolCount; i++)
        {
            if (!prefabs[i]) continue;
            int pool = i;
            pools[i] = new ObjectPool<PoolSpawnableObject>(
                () => Create(pool),
                potion => potion.gameObject.SetActive(true),
                potion => { potion.gameObject.SetActive(false); potion.transform.position = buriedPosition; },
                potion => Destroy(potion.gameObject),
                true, potionsPerDrop, potionsPerDrop * 2);

            pools[i].Prewarm(potionsPerDrop);
        }

        InvokeRepeating(nameof(CheckPools), checkInterval, checkInterval);
    }

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

        if (!roundSystem.isCounting) return false;

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

    private void Drop(int pool)
    {
        int source = PlayerClasses.Chosen == PlayerClassId.Umbral && pools[Umbral] != null ? Umbral : pool;
        if (pools[source] == null) return;
        spawner.ScatterSpawn(pools[source].Get, potionsPerDrop, IsOutOfSight);
    }

    private bool IsOutOfSight(Vector3 point)
    {
        if (!playerCamera) return true;

        Vector3 viewport = playerCamera.WorldToViewportPoint(point);
        bool inFrustum = viewport.z > 0f
                         && viewport.x > -viewMargin && viewport.x < 1f + viewMargin
                         && viewport.y > -viewMargin && viewport.y < 1f + viewMargin;
        if (!inFrustum) return true;

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
