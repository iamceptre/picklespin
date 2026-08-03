using UnityEngine;

public class WinGateKeySpawner : MonoBehaviour
{
    [SerializeField] private GameObject winGateKey;
    [SerializeField] private Transform[] spawnPoints;
    private KeyHasBeenSpawned keySpawnedTip;

    private void Start()
    {
      keySpawnedTip = KeyHasBeenSpawned.instance;
    }


    public void SpawnWinGateKey()
    {
        if (!winGateKey.activeInHierarchy)
        {
            winGateKey.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            winGateKey.SetActive(true);
            keySpawnedTip.Animate();
        }
    }

}
