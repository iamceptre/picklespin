using UnityEngine;
using DG.Tweening;

public class WhiteEnemyEyeLitAnimation : MonoBehaviour
{
    private float animationTime = 3;

    private SpriteRenderer _sprite;
    private Transform _transform;
    private Color startingColor;
    private Color endColor;

    private Vector3 startingScale = new Vector3(0, 0.025f, 1);
    private Vector3 endScale = new Vector3(1, 0.04f, 1);



    public void Flash()
    {
        if (_sprite == null)
        {
            Initialize();
        }

        _sprite.DOKill();
        _transform.DOKill();
        _transform.localScale = startingScale;
        _sprite.color = startingColor;
        _sprite.DOColor(endColor, animationTime).SetEase(Ease.OutExpo);
        _transform.DOScale(endScale, animationTime).SetEase(Ease.OutExpo);
    }


    private void Initialize()
    {
            _sprite = GetComponent<SpriteRenderer>();
            _transform = transform;
            startingColor = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 1);
            endColor = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 0);
    }

}
