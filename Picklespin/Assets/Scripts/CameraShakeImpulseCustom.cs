using UnityEngine;

public class CameraShakeImpulseCustom : MonoBehaviour
{

    [SerializeField] private bool basedOnDistance;

    //private CameraShake cameraShake;
    private CachedCameraMain cachedCameraMain;

    [SerializeField] private float shakeStrenght;
    [SerializeField] private float shakeDuration;

    private void Start()
    {
        //cameraShake = CameraShake.instance;
        cachedCameraMain = CachedCameraMain.instance;
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
        float distance = Vector3.Distance(cachedCameraMain.cachedTransform.position, gameObject.transform.position);
        cameraShake.ExplosionNearbyShake(distance, shakeStrenght);
    }
    */

    private void SendShakeImpulseDistance()
    {
        float distance = Vector3.Distance(cachedCameraMain.cachedTransform.position, gameObject.transform.position);
        //cameraShake.EnemyExplosionShake(distance, shakeStrenght, shakeDuration);
    }

}
