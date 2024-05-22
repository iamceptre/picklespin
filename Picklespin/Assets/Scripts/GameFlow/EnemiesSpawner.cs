using UnityEngine;

public class EnemiesSpawner : MonoBehaviour
{
    //public int howManyToSpawn;

    [SerializeField] private GameObject evilEntity;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private Transform[] waypointsToPass;


    public void SpawnEnemiesEasy(int howManyToSpawn)
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            Invoke("InstantiateEnemy", i * 0.1f);
        }
    }

    private void InstantiateEnemy()
    {
        int randPoint = Random.Range(0, spawnPoints.Length);
        Vector3 randomOffset = Random.insideUnitSphere * 0.5f; 
        Vector3 spawnPosition = spawnPoints[randPoint].position + randomOffset;
        var spawnedOne =  Instantiate(evilEntity, spawnPosition, Quaternion.identity);
        spawnedOne.gameObject.GetComponentInChildren<WaypointsForSpawner>().cachedPoint = waypointsToPass;
    }
}
