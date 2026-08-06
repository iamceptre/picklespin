using DG.Tweening;
using UnityEngine;

public class GrowOnEnable : MonoBehaviour
{
    private const float growDuration = 0.3f;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, growDuration).SetEase(Ease.InOutSine);
    }

    private void OnDisable() => transform.DOKill();
}
