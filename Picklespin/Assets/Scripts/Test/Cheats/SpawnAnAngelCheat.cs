using UnityEngine;

public class SpawnAnAngelCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private AngelSpawner angelSpawner;

    private CheatActivatedFeedback cheatFeedback;


    private void Start()
    {
        cheatFeedback = CheatActivatedFeedback.instance;
        DevLog.Info($"{nameof(SpawnAnAngelCheat)} armed: Up Arrow + A forces an angel spawn", this);
    }

    void Update()
    {
        if (InputCompat.GetKey(KeyCode.UpArrow))
        {
            if (InputCompat.GetKeyDown(KeyCode.A))
            {
                cheatFeedback.Do("forced angel spawn");
                angelSpawner.SpawnAngel();
            }
        }
    }
#endif
}
