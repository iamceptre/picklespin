using UnityEngine;

public class SpawnAnAngelCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private AngelSpawner angelSpawner;

    private CheatActivatedFeedback cheatFeedback;


    private void Start()
    {
        cheatFeedback = CheatActivatedFeedback.instance;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                cheatFeedback.Do("forced angel spawn");
                angelSpawner.SpawnAngel();
            }
        }
    }
#endif
}
