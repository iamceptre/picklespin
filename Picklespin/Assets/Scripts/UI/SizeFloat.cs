using UnityEngine;
using DG.Tweening;

public class SizeFloat : MonoBehaviour
{
    private Vector3 startingSize;
    private Transform myTransform;

    [SerializeField] private float animationTime = 1f;
    [SerializeField] private float howMuchToScaleIn;

    private void Awake()
    {
        myTransform = transform;
    }

    void Start()
    {
        startingSize = myTransform.localScale;
    }


    public void Animate()
    {
        myTransform.DOKill();
        myTransform.localScale = startingSize;
        myTransform.DOScale(startingSize + new Vector3(howMuchToScaleIn, howMuchToScaleIn, howMuchToScaleIn), animationTime).SetLoops(-1, LoopType.Yoyo);
    }

    public void KillTween()
    {
        myTransform.DOKill();
    }



}
