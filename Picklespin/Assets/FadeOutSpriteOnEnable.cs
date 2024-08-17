using UnityEngine;
using DG.Tweening;

public class FadeOutSpriteOnEnable : MonoBehaviour
{
    private SpriteRenderer _sprite;
    private Color targetColor;
    private Color startColor;


    private void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        startColor = _sprite.color;
        targetColor = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, 0);

    }
    private void OnEnable()
    {
        _sprite.color = startColor;

        _sprite.DOColor(targetColor, 1).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
