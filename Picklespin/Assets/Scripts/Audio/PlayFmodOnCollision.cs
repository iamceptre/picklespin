using UnityEngine;
using FMODUnity;
public class PlayFmodOnCollision : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter fmodEmitter;
   // [SerializeField] private string collisionTag = "Concrete";
    [SerializeField] private float startDelay = 0.15f;

    [SerializeField] private float minimumVolume = -9.0f;
    [SerializeField] private float minimumVolumeVelocity = 1.0f;

    [SerializeField] private float maxVolume = 0;
    [SerializeField] private float maxVolumeVelocity = 5.0f;

    private float startTime;

    [SerializeField] private float velocityTreshold = 0.2f;

    private void Start()
    {
        startTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - startTime < startDelay)
        {
            return;
        }

        //Debug.Log(collision.relativeVelocity.magnitude);

        if (collision.relativeVelocity.magnitude >= velocityTreshold)
        {
            float velocityMagnitude = collision.relativeVelocity.magnitude;


            float volume = Mathf.Lerp(minimumVolume, maxVolume, Mathf.InverseLerp(minimumVolumeVelocity, maxVolumeVelocity, velocityMagnitude));

            fmodEmitter.SetParameter("Volume", volume);

            //Debug.Log(volume);

            fmodEmitter.Play();
        }
    }
}
