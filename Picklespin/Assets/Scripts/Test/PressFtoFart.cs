using UnityEngine;
using FMODUnity;


public class PressFtoFart : MonoBehaviour
{
    public EventReference PierdEvent;
    public EventReference MoanEvent;

    void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            RuntimeManager.PlayOneShot(PierdEvent);
        }

        if (Input.GetKeyDown("b"))
        {
            RuntimeManager.PlayOneShot(MoanEvent);
        }

    }
}
