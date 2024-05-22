using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PickupableBonusesSpawner : MonoBehaviour
{
    public static PickupableBonusesSpawner instance { get; private set; }

    public int howManyToSpawn;

    [SerializeField] private GameObject[] bonuses;

    [SerializeField] private Transform[] spawnPoints;
    public bool[] isSpawnPointTaken;

    [SerializeField] private List<int> generatedNumbers;

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
    }
    private void Start()
    {
        isSpawnPointTaken = new bool[spawnPoints.Length];
    }

    public void SetSpawnCount(int spawnCount)
    {
        howManyToSpawn = spawnCount;

        if (howManyToSpawn > spawnPoints.Length)
        {
            howManyToSpawn = spawnPoints.Length;
        }
    }


    public void SpawnBonuses()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            generatedNumbers.Clear();
            StartCoroutine(WaitAndSpawn(i));
        }
    }


    private IEnumerator WaitAndSpawn(int i)
    {
        yield return new WaitForSeconds(i * 0.05f);
        RandomizeWithoutReps();
        var spawned = Instantiate(bonuses[Random.Range(0, bonuses.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
        spawned.GetComponent<FreeUpWaypointAfterPickingUp>().myOccupiedWaypoint = generatedNumbers[i];
        isSpawnPointTaken[generatedNumbers[i]] = true;
        howManyToSpawn--;
    }





    private void RandomizeWithoutReps()
    {
        int maxRange = spawnPoints.Length;
        int minRange = 0;


        rrrandom = Random.Range(minRange, maxRange);

        while (generatedNumbers.Contains(rrrandom) || isSpawnPointTaken[rrrandom])  //maybe otimize it don't have to random so much numbers before getting the one that fits
        {
            rrrandom = Random.Range(minRange, maxRange);
        }

        generatedNumbers.Add(rrrandom);

    }
}
