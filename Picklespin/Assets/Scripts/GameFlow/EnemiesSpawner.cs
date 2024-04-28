using UnityEngine;

public class EnemiesSpawner : MonoBehaviour
{
    public int howManyToSpawn;

    [SerializeField] private GameObject evilEntity;
    [SerializeField] private Transform[] spawnPoints;

    public void SpawnEnemiesEasy()
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
        Instantiate(evilEntity, spawnPosition, Quaternion.identity);
    }
}
