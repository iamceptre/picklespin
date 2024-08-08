using UnityEngine;
using DG.Tweening;


public class DissolveBounce : MonoBehaviour
{
    [SerializeField] private Renderer myRenderer;
    private float dissolve;
    private Tween myTween;
    private int progress = Shader.PropertyToID("_DissolveAmount");

    [SerializeField] private float minDissolve = 0;
    [SerializeField] private float maxDissolve = 1;

    [SerializeField] private float animationTime = 2;

    [SerializeField] private bool startAtStart = false;

    void Start()
    {
        if (startAtStart)
        {
            FireUp();
        }
    }

    public void FadeInAndStart()
    {
        dissolve = 1;

        myTween = DOTween.To(() => dissolve, x => dissolve = x, maxDissolve, animationTime).OnUpdate(() =>
        {
            myRenderer.material.SetFloat(progress, dissolve);
        }).OnComplete(() =>
        {
            FireUp();
        });
    }

    private void FireUp()
    {
        dissolve = maxDissolve;
        myRenderer.material.SetFloat(progress, dissolve);

        myTween = DOTween.To(() => dissolve, x => dissolve = x, minDissolve, animationTime).SetLoops(-1, LoopType.Yoyo).OnUpdate(() =>
        {
            myRenderer.material.SetFloat(progress, dissolve);
        });
    }


}
