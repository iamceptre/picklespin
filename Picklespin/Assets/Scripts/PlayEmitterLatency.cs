using FMODUnity;
using System.Collections;
using UnityEngine;

public class PlayEmitterLatency : MonoBehaviour
{
    private StudioEventEmitter _emitter;

    private void Awake()
    {
        _emitter = GetComponent<StudioEventEmitter>();
    }

    public void Play()
    {
        StartCoroutine(Do());
    }

    private IEnumerator Do()
    {
        yield return null;
        _emitter.Play();
    }
}
