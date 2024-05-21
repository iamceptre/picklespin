using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance { get; private set; }
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform hand;

    private Tween shootTween;
    private Tween explosionTween;
    private Tween enemyDeadExplosionTween;

    private Vector3 startMainCameraRotation;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        startMainCameraRotation = mainCamera.eulerAngles;
    }

    private void ResetShakePosition()
    {
        mainCamera.localEulerAngles = startMainCameraRotation;
    }

    public void ShootCameraShake()
    {
        ResetShakePosition();
        if (shootTween != null)
        {
            shootTween.Kill();
        }
        shootTween = mainCamera.DOShakeRotation(0.25f, 0.8f , 30, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOKill();
        hand.DOShakePosition(0.15f, 0.2f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }



    public void LandCameraShake(float landShakeStrenght)
    {
        ResetShakePosition();
        mainCamera.DOShakeRotation(landShakeStrenght * 0.03f + 0.05f, 0.1f * landShakeStrenght , 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOKill();
        hand.DOShakePosition(0.15f, 0.02f * landShakeStrenght , 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }


    public void ExplosionNearbyShake(float distanceFromExplosion, float damage) //maybe do a delay based on distance, so wait until the shockwave of an explosion gets to you
    {
        ResetShakePosition();
        if (distanceFromExplosion < 70 && damage > 0)
        {
            damage = damage * 0.05f;
            distanceFromExplosion = Mathf.Clamp(distanceFromExplosion, 5, 75);
            float strenghtBasedOnDistance = (75 - distanceFromExplosion) * 0.01f * damage;
            strenghtBasedOnDistance = Mathf.Clamp(strenghtBasedOnDistance, 0, 10);

            //Debug.Log("ran ExplosionShake" + "distance: " + distanceFromExplosion + " str: " + strenghtBasedOnDistance);

            explosionTween.Kill();
            explosionTween = mainCamera.DOShakeRotation(strenghtBasedOnDistance, strenghtBasedOnDistance + damage, 26, 90, true, ShakeRandomnessMode.Harmonic);
            hand.DOKill();
            hand.DOShakePosition(strenghtBasedOnDistance, strenghtBasedOnDistance * 0.02f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
        }
    }

    public void EnemyExplosionShake(float distanceFromExplosion, float explosionStrenght, float explosionDuration) 
    {
        ResetShakePosition();
        float maxRange = 60;
        float minRange = 3;
        float rangesCombined = maxRange + minRange;

        if (distanceFromExplosion < maxRange && explosionStrenght > 0)
        {
            distanceFromExplosion = Mathf.Clamp(distanceFromExplosion, minRange, rangesCombined);
            float strenghtBasedOnDistance = (rangesCombined - distanceFromExplosion) * 0.01f * explosionStrenght;
            strenghtBasedOnDistance = Mathf.Clamp(strenghtBasedOnDistance, 0, 10);

            explosionDuration = explosionDuration + (strenghtBasedOnDistance) * 0.05f;
            enemyDeadExplosionTween.Kill();
            StartCoroutine(EnemyExplosionLater(explosionDuration, strenghtBasedOnDistance));
        }
    }


    private IEnumerator EnemyExplosionLater(float explosionDuration, float strenghtBasedOnDistance) //fix problem of conflict
    {
        ResetShakePosition();
        yield return new WaitForEndOfFrame();
        //can wait time based on distance if we want to
        enemyDeadExplosionTween = mainCamera.DOShakeRotation(explosionDuration, strenghtBasedOnDistance, 35, 90, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.OutExpo);
        //hand.DOShakePosition(explosionDuration, strenghtBasedOnDistance  * 0.02f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }



}
