using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

public class SpellSpawner : MonoBehaviour
{
    public static SpellSpawner instance;

    [SerializeField, Tooltip("one pickup prefab per spell - every spawn, round or on request, comes from these")]
    private SpellPickable[] spellPickups;
    [SerializeField, Tooltip("the spells a round may drop - leave one out to make it reachable only through SpawnSpell")]
    private SpellId[] roundRotation;
    [SerializeField] private Transform[] spawnPoints;

    private SpawnPoints points;
    private readonly Dictionary<SpellId, ObjectPool<SpellPickable>> pools = new();
    private readonly WaitForSeconds scatterTime = new(0.1f);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        points = new SpawnPoints(spawnPoints);

        for (int i = 0; i < spellPickups.Length; i++)
        {
            SpellPickable prefab = spellPickups[i];

            if (!prefab || pools.ContainsKey(prefab.Spell)) continue;

            pools.Add(prefab.Spell, CreatePool(prefab));
        }
    }

    private void Start()
    {
        foreach (KeyValuePair<SpellId, ObjectPool<SpellPickable>> pool in pools)
        {
            pool.Value.Prewarm(InRoundRotation(pool.Key) ? spawnPoints.Length : 1);
        }
    }

    private bool InRoundRotation(SpellId spell)
    {
        for (int i = 0; i < roundRotation.Length; i++)
        {
            if (roundRotation[i] == spell) return true;
        }

        return false;
    }

    private ObjectPool<SpellPickable> CreatePool(SpellPickable prefab)
    {
        ObjectPool<SpellPickable> pool = null;

        pool = new ObjectPool<SpellPickable>(
            () =>
            {
                SpellPickable itemInstance = Instantiate(prefab);
                itemInstance.SetPool(pool);
                return itemInstance;
            },
            item => item.gameObject.SetActive(true),
            item => item.gameObject.SetActive(false),
            item => Destroy(item.gameObject),
            false, spawnPoints.Length, spawnPoints.Length * 2);

        return pool;
    }

    public void SpawnSpellsLo(int howManyToSpawn)
    {
        StartCoroutine(SpawnRoutine(howManyToSpawn));
    }

    private IEnumerator SpawnRoutine(int howManyToSpawn)
    {
        if (roundRotation.Length == 0)
        {
            DevLog.Warn($"{nameof(SpellSpawner)} has no spells in its round rotation", this);
            yield break;
        }

        for (int i = 0; i < howManyToSpawn; i++)
        {
            yield return scatterTime;

            if (!SpawnSpell(roundRotation[Random.Range(0, roundRotation.Length)])) yield break;
        }
    }

    public bool SpawnSpell(SpellId spell)
    {
        if (!pools.TryGetValue(spell, out ObjectPool<SpellPickable> pool))
        {
            DevLog.Warn($"{nameof(SpellSpawner)} has no pickup prefab for {spell}", this);
            return false;
        }

        if (!points.TryReserve(out int point)) return false;

        pool.Get().PlaceAt(points.PositionOf(point), point, this);
        return true;
    }

    public void ReleasePoint(int point) => points.Release(point);
}
