using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(StudioEventEmitter))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]

public class PlayEmitterOnTriggerEnter : MonoBehaviour
{

    private StudioEventEmitter _emitter;

    private void Awake()
    {
        _emitter = GetComponent<StudioEventEmitter>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _emitter.Play();
        }
    }
}
