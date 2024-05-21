using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SpellSpawner : MonoBehaviour
{
    [Tooltip("cannot exceed number of spawn points")]public int howManyToSpawn;

    [SerializeField] private GameObject[] spellsLo;
    [SerializeField] private GameObject[] spellsHi;

    [SerializeField] private Transform[] spawnPoints;

    private List<int> generatedNumbers = new List<int>();

    private int rrrandom;



    public void SpawnSpellsLo()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            StartCoroutine(WaitAndSpawnLo(i));
        }
    }

    public void SpawnSpellsHi()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            StartCoroutine(WaitAndSpawnHi(i));
        }
    }

    private IEnumerator WaitAndSpawnLo(int i)
    {
        yield return new WaitForSeconds(i * 0.06f);
        RandomizeWithoutReps();
        Instantiate(spellsLo[Random.Range(0, spellsLo.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
    }

    private IEnumerator WaitAndSpawnHi(int i)
    {
        yield return new WaitForSeconds(i * 0.06f);
        RandomizeWithoutReps();
        Instantiate(spellsHi[Random.Range(0, spellsHi.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
    }

    public void SpawnLastSpell()
    {
        Debug.Log("zongi! tyœ wygra³ w tê grê :-D");
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
