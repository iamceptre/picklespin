using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PickupableBonusesSpawner : MonoBehaviour
{
    [Tooltip("cannot exceed number of spawn points")] public int howManyToSpawn;

    [SerializeField] private GameObject[] bonuses;

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private List<int> generatedNumbers;

    private int rrrandom;


    public void SpawnBonuses()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            StartCoroutine(WaitAndSpawn(i));
        }
    }


    private IEnumerator WaitAndSpawn(int i)
    {
        yield return new WaitForSeconds(i * 0.05f);
        RandomizeWithoutReps();
        Instantiate(bonuses[Random.Range(0, bonuses.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
    }





    private void RandomizeWithoutReps()
    {
        int maxRange = spawnPoints.Length;
        int minRange = 0;

        rrrandom = Random.Range(minRange, maxRange);

        while (generatedNumbers.Contains(rrrandom))
        {
            rrrandom = Random.Range(minRange, maxRange);
        }

        generatedNumbers.Add(rrrandom);

    }
}
