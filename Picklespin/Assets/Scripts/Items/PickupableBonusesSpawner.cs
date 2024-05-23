using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PickupableBonusesSpawner : MonoBehaviour
{
    public static PickupableBonusesSpawner instance { get; private set; }

    public int howManyToSpawn;
    [HideInInspector] public int startingHowManyToSpawn;

    [SerializeField] private GameObject[] bonuses;

    public Transform[] spawnPoints;

   public List<Transform> AvaliableSpawnPoints;

    private int rrrandom;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        if (howManyToSpawn > spawnPoints.Length)
        {
            howManyToSpawn = spawnPoints.Length;
        }

        startingHowManyToSpawn = howManyToSpawn;
    }

    private void Start()
    {
        AvaliableSpawnPoints = spawnPoints.ToList();
    }

    public void SetSpawnCount(int spawnCount)
    {
        howManyToSpawn = spawnCount;

        if (howManyToSpawn > spawnPoints.Length)
        {
            howManyToSpawn = spawnPoints.Length;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBonuses();
        }
    }

    public void SpawnBonuses()
    {
            StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            yield return new WaitForSeconds(i * 0.02f);
            Spawn();
        }
        yield return null;
    }

    private void Spawn()
    {
        Randomize();
        var spawned = Instantiate(bonuses[Random.Range(0, bonuses.Length)], AvaliableSpawnPoints[rrrandom].position, Quaternion.identity);
        spawned.GetComponent<FreeUpWaypointAfterPickingUp>().SetOccupiedWaypoint(AvaliableSpawnPoints[rrrandom],this);
        howManyToSpawn = Mathf.Clamp(howManyToSpawn, 0, AvaliableSpawnPoints.Count);
    }

    private void Randomize()
    {
        int maxRange = AvaliableSpawnPoints.Count;
        int minRange = 0;
        rrrandom = Random.Range(minRange, maxRange);
    }
}
