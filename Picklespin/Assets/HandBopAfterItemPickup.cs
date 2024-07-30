using UnityEngine;
using DG.Tweening;

public class HandBopAfterItemPickup : MonoBehaviour
{

    public static HandBopAfterItemPickup instance { get; private set; }

    private Transform _transform;
    private float startingYpos;
    private float bopAmount = 0.1f;

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

        _transform.DOLocalMoveY(startingYpos - bopAmount, 0.1f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _transform.DOLocalMoveY(startingYpos + bopAmount, 0.162f).SetEase(Ease.InOutSine);
        });

    }
}
