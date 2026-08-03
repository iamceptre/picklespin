using UnityEngine;
using DG.Tweening;

public class SizeFloat : MonoBehaviour
{
    private Transform myTransform;
    [SerializeField] private bool startOnAwake = false;

    [SerializeField] private float animationTime = 1f;
    [SerializeField] private float howMuchToScaleIn;

    private void Start()
    {
        myTransform = transform;
        if (startOnAwake) Animate();
    }



    public void Animate()
    {
        myTransform.DOKill();
        myTransform.localScale = Vector3.one;
        myTransform.DOScale(Vector3.one + new Vector3(howMuchToScaleIn, howMuchToScaleIn, howMuchToScaleIn), animationTime).SetLoops(-1, LoopType.Yoyo);
    }

    public void KillTween()
    {
        myTransform.DOKill();
    }



}
