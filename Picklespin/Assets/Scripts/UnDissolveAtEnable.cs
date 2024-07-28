using UnityEngine;
using DG.Tweening;

public class UnDissolveAtEnable : MonoBehaviour
{
    [SerializeField] private float animationTime;
    [SerializeField] private float startingValue = 1;
    [SerializeField] private float targetValue = 0;
    private float t;

    private int dissolveID = Shader.PropertyToID("_DissolveAmount");

    private Renderer _rend;

    private void Awake()
    {
        _rend = GetComponent<Renderer>();   
    }

    private void OnEnable()
    {
        t = startingValue;
        _rend.material.SetFloat(dissolveID, t);
        DOTween.To(() => t, x => t = x, targetValue, animationTime).OnUpdate(() =>
        {
            _rend.material.SetFloat(dissolveID, t);
        });
    }
}
