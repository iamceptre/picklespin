using DG.Tweening;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance { get; private set; }
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform hand;

    private Tween shootTween;
    private Tween handShakeTween;

    private Tween explosionTween;
    private Tween enemyDeadExplosionTween;

    private Vector3 startMainCameraRotation;

    private WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

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
        shootTween = mainCamera.DOShakeRotation(0.3f, 0.8f, 30, 90, true, ShakeRandomnessMode.Harmonic).SetAutoKill(false).Pause();
        handShakeTween = hand.DOShakePosition(0.15f, 0.2f, 40, 90, false, true, ShakeRandomnessMode.Harmonic).SetAutoKill(false).Pause();
    }


    private void RestoreZeroRotation()
    {
        mainCamera.DOLocalRotate(Vector3.zero, 0.1f);
    }


    public void ShootCameraShake()
    {

        shootTween.Restart();
        handShakeTween.Restart();
    }



    public void LandCameraShake(float landShakeStrenght)
    {
        mainCamera.DOShakeRotation(landShakeStrenght * 0.03f + 0.05f, 0.1f * landShakeStrenght , 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(0.15f, 0.02f * landShakeStrenght , 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }


    public void ExplosionNearbyShake(float distanceFromExplosion, float damage) 
    {
        if (distanceFromExplosion < 70 && damage > 0)
        {
            damage = damage * 0.05f;
            distanceFromExplosion = Mathf.Clamp(distanceFromExplosion, 5, 75);
            float strenghtBasedOnDistance = (75 - distanceFromExplosion) * 0.01f * damage;
            strenghtBasedOnDistance = Mathf.Clamp(strenghtBasedOnDistance, 0, 10);

            mainCamera.DOKill();
            explosionTween.Kill();
            explosionTween = mainCamera.DOShakeRotation(strenghtBasedOnDistance, strenghtBasedOnDistance + damage, 26, 90, true, ShakeRandomnessMode.Harmonic).OnComplete(() =>
            {
                RestoreZeroRotation();
            }); ;

            //hand.DOShakePosition(strenghtBasedOnDistance, strenghtBasedOnDistance * 0.02f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
        }
    }

    public void EnemyExplosionShake(float distanceFromExplosion, float explosionStrenght, float explosionDuration) 
    {
        float maxRange = 60;
        float minRange = 3;
        float rangesCombined = maxRange + minRange;

        if (distanceFromExplosion < maxRange && explosionStrenght > 0)
        {
            distanceFromExplosion = Mathf.Clamp(distanceFromExplosion, minRange, rangesCombined);
            float strenghtBasedOnDistance = (rangesCombined - distanceFromExplosion) * 0.01f * explosionStrenght;
            strenghtBasedOnDistance = Mathf.Clamp(strenghtBasedOnDistance, 0, 10);

            explosionDuration = explosionDuration + (strenghtBasedOnDistance) * 0.05f;
            explosionTween.Kill();
            StartCoroutine(EnemyExplosionLater(explosionDuration, strenghtBasedOnDistance));
        }
    }


    private IEnumerator EnemyExplosionLater(float explosionDuration, float strenghtBasedOnDistance) //fix problem of conflict
    {
        yield return waitFrame;
        mainCamera.DOShakeRotation(explosionDuration, strenghtBasedOnDistance, 35, 90, true, ShakeRandomnessMode.Harmonic).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            RestoreZeroRotation();
        });

        //hand.DOShakePosition(explosionDuration, strenghtBasedOnDistance  * 0.02f, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }



}
