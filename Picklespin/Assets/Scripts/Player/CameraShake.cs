using DG.Tweening;
using System;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform hand;

    [SerializeField] private float positionStrenght;
    [SerializeField] private float rotationStrenght;

    public float shakeDuration;

    public float shakeMultiplier;

    private static event Action Shake;

    public static void Invoke()
    {
        Shake?.Invoke();
    }


    private void OnEnable() => Shake += DoCameraShakeShoot;
    private void OnDisable() => Shake -= DoCameraShakeShoot;

    private void DoLandShake()
    {
       hand.DOShakePosition(shakeDuration, positionStrenght* 9999);
    }


    private void DoCameraShakeShoot()
    {
        mainCamera.DOShakeRotation(shakeDuration, rotationStrenght * shakeMultiplier, 26, 90, true, ShakeRandomnessMode.Harmonic);
        hand.DOShakePosition(shakeDuration * 1.3f, positionStrenght, 40, 90, false, true, ShakeRandomnessMode.Harmonic);
    }
    

}
