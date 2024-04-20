using UnityEngine;

public class EnemiesSpawner : MonoBehaviour
{

    public int howManyToSpawn;

    [SerializeField] private GameObject evilEntity;
    [SerializeField] private Transform[] spawnPoints;

    private Transform t_spawnPoint;
    private Vector3 t_spawnPosVector;



    public void SpawnEnemiesEasy()
    {
        //Debug.Log("Spawning Enemies Easy");

        for (int i = 0; i < howManyToSpawn; i++)
        {
            RandomizeSpawnPoint();
            Instantiate(evilEntity, t_spawnPosVector, Quaternion.identity);
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnEnemiesEasy();
        }
    }

    private void RandomizeSpawnPoint()
    {
        int randPoint = Random.Range(0, spawnPoints.Length);
        t_spawnPoint = spawnPoints[randPoint];
        t_spawnPosVector = t_spawnPoint.position + new Vector3(Random.Range(0, 0.5f), Random.Range(0, 0.5f), Random.Range(0, 0.5f));
    }

}