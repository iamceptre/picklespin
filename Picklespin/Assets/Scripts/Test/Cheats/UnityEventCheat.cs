using UnityEngine;
using UnityEngine.Events;

public class UnityEventCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private string cheatName = "my name";

    [SerializeField] private KeyCode cheatKey = KeyCode.X;

    [SerializeField] private UnityEvent _event; 

    private CheatActivatedFeedback cheatActivatedFeedback;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
    }

    void Update()
    {

        if(InputCompat.GetKey(KeyCode.UpArrow))
        {
            if (InputCompat.GetKeyDown(cheatKey))
            {
                _event.Invoke();
                cheatActivatedFeedback.Do(cheatName);
            }
        }

    }
#endif
}
