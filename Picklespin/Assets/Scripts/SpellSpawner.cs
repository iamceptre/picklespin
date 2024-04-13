using System.Collections.Generic;
using UnityEngine;

public class SpellSpawner : MonoBehaviour
{
    [Tooltip("cannot exceed number of spawn points")]public int howManyToSpawn;

    [SerializeField] private GameObject[] spellsLo;
    [SerializeField] private GameObject[] spellsHi;

    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private List<int> generatedNumbers;

    private int rrrandom;


    public void SpawnSpellsLo()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            RandomizeWithoutReps();
            Instantiate(spellsLo[Random.Range(0,spellsLo.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
        }
    }

    public void SpawnSpellsHi()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            RandomizeWithoutReps();
            Instantiate(spellsHi[Random.Range(0, spellsHi.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
        }
    }

    public void SpawnLastSpell()
    {
        Debug.Log("zongi! ty� wygra� w t� gr� :-D");
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
