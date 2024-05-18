using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ExplosionScaleTween : MonoBehaviour
{
    private Renderer _renderer;
    private Color _color = Color.white;
    private float animationProgress = 0;
    private float animationTime = 0.3f;
    private void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        StartCoroutine(FadeOutMaterial());
       float distanceToPlayer = Vector3.Distance(transform.position, Camera.main.transform.position);

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
            //Debug.Log(_color);
            _renderer.material.SetColor("_Color", _color);
            yield return null;
        }
    }
}
