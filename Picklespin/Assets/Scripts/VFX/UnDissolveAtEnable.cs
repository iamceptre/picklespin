using UnityEngine;
using DG.Tweening;

public class UnDissolveAtEnable : MonoBehaviour
{
    [SerializeField] private float animationTime;
    [SerializeField] private float startingValue = 1;
    [SerializeField] private float targetValue = 0;
    private float t;

    private static readonly int dissolveID = Shader.PropertyToID("_DissolveAmount");

    private Material materialInstance;
    private Tween unDissolveTween;

    private void Awake()
    {
        materialInstance = GetComponent<Renderer>().material;
    }

    private void OnEnable()
    {
        t = startingValue;
        materialInstance.SetFloat(dissolveID, t);
        unDissolveTween = DOTween.To(() => t, x => t = x, targetValue, animationTime).OnUpdate(() =>
        {
            materialInstance.SetFloat(dissolveID, t);
        });
    }

    private void OnDisable()
    {
        unDissolveTween?.Kill();
        unDissolveTween = null;
    }
}
