using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ExplosionScaleTween : MonoBehaviour
{
    private Material materialInstance;
    private Color _color = Color.white;
    private float animationProgress = 0;
    private float animationTime = 0.3f;

    private static readonly int colorID = Shader.PropertyToID("_Color");

    private void OnEnable()
    {
        materialInstance = GetComponent<Renderer>().material;
        StartCoroutine(FadeOutMaterial());
       float distanceToPlayer = Vector3.Distance(transform.position, CachedCameraMain.instance.cachedTransform.position);

        transform.DOScale(distanceToPlayer * 2, animationTime).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            transform.DOKill();
            Destroy(gameObject);
        });


    }

    private IEnumerator FadeOutMaterial()
    {
        while (animationProgress < animationTime)
        {
            float progressPercentage = animationProgress/animationTime; //1 is done
            animationProgress += Time.deltaTime;
            _color = new Color(0.5f, 0.5f, 0.5f, 1 - progressPercentage);
            materialInstance.SetColor(colorID, _color);
            yield return null;
        }
    }
}
