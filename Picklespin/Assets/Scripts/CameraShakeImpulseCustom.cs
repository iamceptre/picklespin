using UnityEngine;

public class CameraShakeImpulseCustom : MonoBehaviour
{

    [SerializeField] private bool basedOnDistance;

    private CameraShake cameraShake;

    [SerializeField] private float shakeStrenght;
    [SerializeField] private float shakeDuration;

    private void Start()
    {
        cameraShake = CameraShake.instance;
    }

    public void Shake()
    {
        /*
        if (basedOnDistance)
        {
            SendShakeImpulseDistance();
        }
        else
        {
            SendShakeImpulse();
        }
        */

        SendShakeImpulseDistance();
    }

    /*
    private void SendShakeImpulse()
    {
        cameraShake.CustomCameraShake(shakeStrenght, shakeDuration);
    }

    private void SendShakeImpulseDistance()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);
        cameraShake.ExplosionNearbyShake(distance, shakeStrenght);
    }
    */

    private void SendShakeImpulseDistance()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);
        cameraShake.EnemyExplosionShake(distance, shakeStrenght, shakeDuration);
    }

}
