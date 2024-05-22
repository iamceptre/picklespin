using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableBonusesSpawner : MonoBehaviour
{
    public static PickupableBonusesSpawner instance { get; private set; }

    public int howManyToSpawn;
    [HideInInspector] public int startingHowManyToSpawn;

    [SerializeField] private GameObject[] bonuses;

    [SerializeField] private Transform[] spawnPoints;

    public List<int> TakenSpawnPoints = new List<int>();

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
            yield return new WaitForSeconds(i * 0.03f);
            Spawn();
        }
        yield return null;
    }

    private void Spawn()
    {
        RandomizeWithoutReps();
        var spawned = Instantiate(bonuses[Random.Range(0, bonuses.Length)], spawnPoints[rrrandom].position, Quaternion.identity);
        spawned.GetComponent<FreeUpWaypointAfterPickingUp>().myOccupiedWaypoint = rrrandom;
        howManyToSpawn = Mathf.Clamp(howManyToSpawn, 0, spawnPoints.Length - TakenSpawnPoints.Count);
    }

    private void RandomizeWithoutReps()
    {
        int maxRange = spawnPoints.Length;
        int minRange = 0;


        rrrandom = Random.Range(minRange, maxRange);

        while (TakenSpawnPoints.Contains(rrrandom))
        {
            rrrandom = Random.Range(minRange, maxRange);
        }

        TakenSpawnPoints.Add(rrrandom);
    }
}
