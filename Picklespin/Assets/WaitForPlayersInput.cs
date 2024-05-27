using UnityEngine;
using UnityEngine.Events;

public class WaitForPlayersInput : MonoBehaviour
{

    [SerializeField] private UnityEvent afterPlayerInputEvent;
    private CheatActivatedFeedback cheatActivatedFeedback;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
    }

    private void Update()
    {
        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            afterPlayerInputEvent.Invoke();
            enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            cheatActivatedFeedback.Do("rounds disabled");
            enabled = false;
        }
    }

}
