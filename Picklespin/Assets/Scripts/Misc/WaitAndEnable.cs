using UnityEngine;

public class WaitAndEnable : MonoBehaviour
{

    [SerializeField] private float waitTime;
    [Header("Random")]
    [SerializeField] private bool randomize;
    [SerializeField] private float randomMinTime;
    [SerializeField] private float randomMaxTime;

    void Start()
    {
        if (gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }

        if (!randomize) {
            Invoke("Activate", waitTime);
        }
        else
        {
            Invoke("Activate", Random.Range(randomMinTime, randomMaxTime));
        }
    }

    private void Activate()
    {
        gameObject.SetActive (true);
    }

}
