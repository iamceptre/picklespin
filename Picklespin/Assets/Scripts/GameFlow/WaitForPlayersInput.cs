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
        if (InputCompat.GetAxisRaw("Vertical") != 0 || InputCompat.GetAxisRaw("Horizontal") != 0)
        {
            afterPlayerInputEvent.Invoke();
            enabled = false;
        }

        if (InputCompat.GetKeyDown(KeyCode.T))
        {
            cheatActivatedFeedback.Do("rounds disabled");
            enabled = false;
        }
    }

}
