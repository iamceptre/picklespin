using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SpellSpawner : MonoBehaviour
{
    public static SpellSpawner instance;


    public int howManyToSpawn;

    [SerializeField] private GameObject[] spellsLo;
    [SerializeField] private GameObject[] spellsHi;

    [SerializeField] private Transform[] spawnPoints;

    private List<int> generatedNumbers = new List<int>();

    private int rrrandom;

    public bool[] isSpawnPointTaken;

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



    public void SpawnSpellsLo()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            StartCoroutine(WaitAndSpawnLo(i));
        }
    }




    private IEnumerator WaitAndSpawnLo(int i)
    {
        yield return new WaitForSeconds(i * 0.06f);
        RandomizeWithoutReps();
        var spawnedSpell = Instantiate(spellsLo[Random.Range(0, spellsLo.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
        var freeUpScript = spawnedSpell.GetComponent<FreeUpWaypointAfterPickingUp>();
        //freeUpScript.myOccupiedWaypoint = generatedNumbers[i];

        howManyToSpawn--;
    }

    /*
    private IEnumerator WaitAndSpawnHi(int i)
    {
        yield return new WaitForSeconds(i * 0.06f);
        RandomizeWithoutReps();
        Instantiate(spellsHi[Random.Range(0, spellsHi.Length)], spawnPoints[generatedNumbers[i]].position, Quaternion.identity);
    }

    */

    /*

public void SpawnSpellsHi()
{
    for (int i = 0; i < howManyToSpawn; i++)
    {
        StartCoroutine(WaitAndSpawnHi(i));
    }
}

*/

    public void SpawnLastSpell()
    {
        Debug.Log("zongi! tyœ wygra³ w tê grê :-D");
    }



    private void RandomizeWithoutReps()
    {
        int maxRange = spawnPoints.Length;
        int minRange = 0;

        rrrandom = Random.Range(minRange, maxRange);

        while (generatedNumbers.Contains(rrrandom) || isSpawnPointTaken[rrrandom])
        {
            rrrandom = Random.Range(minRange, maxRange);
        }

        generatedNumbers.Add(rrrandom);

    }
}
