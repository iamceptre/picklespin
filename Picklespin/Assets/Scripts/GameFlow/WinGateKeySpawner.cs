using UnityEngine;

public class WinGateKeySpawner : MonoBehaviour
{
    [SerializeField] private GameObject winGateKey;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private GameObject enableAfterPickingUpTheKey;

    private WinGateKeyItem keyItemScript;
    private KeyHasBeenSpawned keySpawnedTip;

    private bool alreadySpawnedAKey = false;

    private void Start()
    {
      keySpawnedTip = KeyHasBeenSpawned.instance;
    }


    public void SpawnWinGateKey()
    {
        if (!alreadySpawnedAKey)
        {
            var spawnedKey = Instantiate(winGateKey, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
            spawnedKey.transform.SetParent(transform);
            keySpawnedTip.gameObject.SetActive(true);
            keySpawnedTip.Animate();
            keyItemScript = spawnedKey.GetComponent<WinGateKeyItem>();
            keyItemScript.toEnable = enableAfterPickingUpTheKey;
        }
    }

}
