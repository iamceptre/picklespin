using UnityEngine;
using DG.Tweening;

public class DissolveBounce : MonoBehaviour
{
    [SerializeField] private Renderer myRenderer;
    private Material myMaterial;
    private float dissolve;
    private Tween myTween;
    private static readonly int Progress = Shader.PropertyToID("_DissolveAmount");

    [SerializeField] private float minDissolve = 0f;
    [SerializeField] private float maxDissolve = 1f;
    [SerializeField] private float animationTime = 2f;
    [SerializeField] private bool startAtStart = false;

    private bool isVisible = false;

    void Start()
    {
        myMaterial = myRenderer.material;

        if (startAtStart)
        {
            FadeInAndStart();
        }
    }

    public void FadeInAndStart()
    {
        if (myTween != null && myTween.IsActive())
        {
            myTween.Kill();
        }

        dissolve = 1f;

        myTween = DOTween.To(() => dissolve, x => dissolve = x, maxDissolve, animationTime)
            .OnUpdate(() =>
            {
                myMaterial.SetFloat(Progress, dissolve);
            })
            .OnComplete(() =>
            {
                FireUp();
            });
    }

    private void FireUp()
    {
        dissolve = maxDissolve;
        myMaterial.SetFloat(Progress, dissolve);

        myTween = DOTween.To(() => dissolve, x => dissolve = x, minDissolve, animationTime)
            .SetLoops(-1, LoopType.Yoyo)
            .OnUpdate(() =>
            {
                myMaterial.SetFloat(Progress, dissolve);
            });

        if (!isVisible)
        {
            myTween.Pause();
        }
    }

    void OnBecameVisible()
    {
        isVisible = true;
        if (myTween != null && myTween.IsActive())
        {
            myTween.Play();
        }
    }

    void OnBecameInvisible()
    {
        isVisible = false;
        if (myTween != null && myTween.IsActive())
        {
            myTween.Pause();
        }
    }

    public void FadeOutAndDisable()
    {
        if (myTween != null && myTween.IsActive())
        {
            myTween.Kill();
        }


        myTween = DOTween.To(() => dissolve, x => dissolve = x, 1, animationTime*0.5f)
            .OnUpdate(() =>
            {
                myMaterial.SetFloat(Progress, dissolve);
            })
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}