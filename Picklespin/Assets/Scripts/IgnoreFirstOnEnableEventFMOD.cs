using FMODUnity;
using UnityEngine;

public class IgnoreFirstOnEnableEventFMOD : MonoBehaviour
{

    [SerializeField] private StudioEventEmitter _emitter;
    private bool ignoreFirst = true;
    
    private void OnEnable()
    {
        if (ignoreFirst)
        {
            ignoreFirst = false;
            return;
        }
        else
        {
            _emitter.Play();
        }
    }

}
