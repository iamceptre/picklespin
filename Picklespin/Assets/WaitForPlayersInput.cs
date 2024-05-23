using UnityEngine;
using UnityEngine.Events;

public class WaitForPlayersInput : MonoBehaviour
{

    [SerializeField] private UnityEvent afterPlayerInputEvent;

    private void Update()
    {
        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            afterPlayerInputEvent.Invoke();
            enabled = false;
        }
    }

}
