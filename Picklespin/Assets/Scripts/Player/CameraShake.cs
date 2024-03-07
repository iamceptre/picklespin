using DG.Tweening;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform hand;
    
    public float landShakeStrenght;

    private float fpsCompensate;



    public void ShootCameraShake()
    {
        MakeShakeEqualOnEveryFPS();
        mainCamera.DOShakeRotation(0.17f, 1.2f * fpsCompensate, 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(0.2f, 0.1f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }



    public void LandCameraShake()
    {
        MakeShakeEqualOnEveryFPS();
        mainCamera.DOShakeRotation(landShakeStrenght*0.03f + 0.05f, 0.1f * landShakeStrenght * fpsCompensate, 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(0.15f, 0.02f * landShakeStrenght * fpsCompensate, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }



    public void ExplosionNearbyShake(float distanceFromExplosion) //maybe do a delay based on distance, so wait until the shockwave of an explosion gets to you
    {
        MakeShakeEqualOnEveryFPS();

        distanceFromExplosion = Mathf.Clamp(distanceFromExplosion, 5, 45);

        float strenghtBasedOnDistance = (45 - distanceFromExplosion) * 0.015f;


        mainCamera.DOShakeRotation(strenghtBasedOnDistance * fpsCompensate, strenghtBasedOnDistance * (fpsCompensate * 2), 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(strenghtBasedOnDistance * fpsCompensate, strenghtBasedOnDistance * (fpsCompensate * 2) * 0.02f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }


    private void MakeShakeEqualOnEveryFPS()
    {
        fpsCompensate = 1/(Time.smoothDeltaTime * 60f);
    }


}
