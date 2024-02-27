using DG.Tweening;
using System;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform hand;

    [SerializeField] private Vector3 positionStrenght;
    [SerializeField] private Vector3 rotationStrenght;

    public float shakeDuration;

    public float shakeMultiplier;

    private static event Action Shake;

    public static void Invoke()
    {
        Shake?.Invoke();
    }

    private void OnEnable() => Shake += DoCameraShake;
    private void OnDisable() => Shake -= DoCameraShake;

    private void DoCameraShake()
    {
       // mainCamera.DOShakePosition(shakeDuration, positionStrenght * shakeMultiplier);
        mainCamera.DOShakeRotation(shakeDuration, rotationStrenght* shakeMultiplier);
        hand.DOShakePosition(shakeDuration, positionStrenght* -0.1f);
    }
    

}
