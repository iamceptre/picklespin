//DESTROYS EVERY PREVIOUS LIGHT SO THERES ONLY ONE AT A TIME
using System.Collections;
using UnityEngine;

public class SpelledLightSpawner : MonoBehaviour
{
    private GameObject[] previousOnes;
    [SerializeField] private GameObject lightToSpawn;
    void Awake()
    {
        previousOnes = GameObject.FindGameObjectsWithTag("SpelledLight");
    }

    private void Start()
    {
            foreach (GameObject one in previousOnes)
            {
                LightSpell lightSpell = one.GetComponent<LightSpell>();
                lightSpell.FadeOut();
            }

        StartCoroutine(WaitAndSpawn());
    }

    private IEnumerator WaitAndSpawn()
    {
        yield return new WaitForSeconds(0.2f);
        Instantiate(lightToSpawn,transform.position, Quaternion.identity);
        Destroy(gameObject);
        yield return null;
    }
}
