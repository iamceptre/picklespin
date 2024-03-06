using DG.Tweening;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform hand;
    
    public float landShakeStrenght;


    public void ShootCameraShake()
    {
        mainCamera.DOShakeRotation(0.15f, 1.2f, 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(0.2f, 0.1f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }



    public void LandCameraShake()
    {
        mainCamera.DOShakeRotation(landShakeStrenght*0.05f, 0.1f * landShakeStrenght, 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(0.15f, 0.05f * landShakeStrenght, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }



    public void ExplosionNearbyShake(float distanceFromExplosion) //maybe do a delay based on distance, so wait until the shockwave of an explosion gets to you
    {
        distanceFromExplosion = Mathf.Clamp(distanceFromExplosion, 5, 45);

        float strenghtBasedOnDistance = (45 - distanceFromExplosion) * 0.01f;


        mainCamera.DOShakeRotation(strenghtBasedOnDistance, strenghtBasedOnDistance, 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(strenghtBasedOnDistance, strenghtBasedOnDistance * 0.02f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }


}
