using UnityEngine;
using FMODUnity;


[RequireComponent(typeof(StudioEventEmitter))]
public class SoundOnCollision : MonoBehaviour
{
    private StudioEventEmitter emitter;
    private Rigidbody rb;
    [SerializeField] private float collisionImpactTreshold;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        emitter = GetComponent<StudioEventEmitter>();
    }



    private void OnCollisionEnter(Collision collision)
    {
        float collisionVelocity = collision.relativeVelocity.magnitude;

        if (collisionVelocity >= collisionImpactTreshold)
        {
            emitter.Play();
        }
    }

    private void Update()
    {
        rb.AddForce(transform.forward*5);
    }


}
