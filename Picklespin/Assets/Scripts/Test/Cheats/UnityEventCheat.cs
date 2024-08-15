using UnityEngine;
using UnityEngine.Events;

public class UnityEventCheat : MonoBehaviour
{
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

        if(Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKeyDown(cheatKey))
            {
                _event.Invoke();
                cheatActivatedFeedback.Do(cheatName);
            }
        }

    }
}
