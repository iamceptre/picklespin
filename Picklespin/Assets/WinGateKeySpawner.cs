using UnityEngine;

public class WinGateKeySpawner : MonoBehaviour
{
    [SerializeField] private GameObject winGateKey;
    [SerializeField] private Transform[] spawnPoints;

    private bool alreadySpawnedAKey = false;


    public void SpawnWinGateKey()
    {
        if (!alreadySpawnedAKey)
        {
            var spawnedKey = Instantiate(winGateKey, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
            spawnedKey.transform.SetParent(transform);
            //show info on gui + sick animation
        }
    }

}
