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
        if (_emitter != null)
        {
            if (!_setOnFire.enabled)
            {
                _emitter.Play();
            }
        }
    }

}
