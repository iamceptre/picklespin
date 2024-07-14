using FMODUnity;
using UnityEngine;

public class DisableFlyBySoundOnExplosion : MonoBehaviour
{

    [SerializeField] private StudioEventEmitter flybyEmitter;

    private void OnEnable()
    {
        flybyEmitter.Stop();  
    }

}
