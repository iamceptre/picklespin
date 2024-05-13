using UnityEngine;
using DG.Tweening;

public class ScaleUpOnAwake : MonoBehaviour
{
    private Vector3 startingScale;
    [SerializeField] private float animationTime;
    private void Awake()
    {
        startingScale = transform.localScale;
    }
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(startingScale, 1).SetEase(Ease.OutExpo);
    }
}
