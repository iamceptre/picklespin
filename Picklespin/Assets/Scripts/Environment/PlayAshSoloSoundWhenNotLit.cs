using FMODUnity;
using UnityEngine;

public class PlayAshSoloSoundWhenNotLit : MonoBehaviour
{

    [SerializeField] private SetOnFire _setOnFire;

    private StudioEventEmitter _emitter;

    private void Awake()
    {
        _emitter = GetComponent<StudioEventEmitter>();
    }


    public void Play()
    {
        if (_emitter == null) return;
        // a body that died alight already has the fire's own ash sound
        if (_setOnFire != null && (_setOnFire.IsBurning || _setOnFire.WasBurningAtDeath)) return;

        _emitter.Play();
    }

}
