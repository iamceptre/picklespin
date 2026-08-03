using UnityEngine;
using DG.Tweening;
using System.Collections;

// One-shot flash, replayed by activating the object. Its owner is pooled, so it
// deactivates rather than destroys itself and re-arms every animated value on enable.
public class ExplosionScaleTween : MonoBehaviour
{
    private Material materialInstance;
    private Transform _transform;
    private Vector3 originalScale;
    private float animationProgress;
    private readonly float animationTime = 0.3f;

    private static readonly int colorID = Shader.PropertyToID("_Color");
    private static readonly Color opaque = new(0.5f, 0.5f, 0.5f, 1f);

    private void Awake()
    {
        _transform = transform;
        originalScale = _transform.localScale;
        materialInstance = GetComponent<Renderer>().material;
    }

    private void OnEnable()
    {
        _transform.DOKill();
        _transform.localScale = originalScale;
        animationProgress = 0f;
        materialInstance.SetColor(colorID, opaque);

        StartCoroutine(FadeOutMaterial());

        float distanceToPlayer = CachedCameraMain.instance
            ? Vector3.Distance(_transform.position, CachedCameraMain.instance.cachedTransform.position)
            : 1f;

        _transform.DOScale(distanceToPlayer * 2f, animationTime)
            .SetEase(Ease.OutExpo)
            .OnComplete(() => gameObject.SetActive(false));
    }

    private void OnDisable()
    {
        if (gameObject.IsUnloading()) return;

        _transform.DOKill();
    }

    private IEnumerator FadeOutMaterial()
    {
        while (animationProgress < animationTime)
        {
            float progressPercentage = animationProgress / animationTime;
            animationProgress += Time.deltaTime;
            materialInstance.SetColor(colorID, new Color(0.5f, 0.5f, 0.5f, 1f - progressPercentage));
            yield return null;
        }
    }
}
