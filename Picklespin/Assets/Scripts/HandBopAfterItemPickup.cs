using UnityEngine;
using DG.Tweening;

public class HandBopAfterItemPickup : MonoBehaviour
{

    public static HandBopAfterItemPickup instance { get; private set; }

    private Transform _transform;
    private float startingYpos;
    private float bopAmount = 0.15f;
    private float bopTime = 0.12f;

    private void Awake()
    {
        _transform = transform;
        startingYpos = _transform.localPosition.y;

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }



    public void Do()
    {
        _transform.DOKill();
        _transform.DOLocalMoveY(startingYpos - bopAmount, bopTime).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo);
    }
}
