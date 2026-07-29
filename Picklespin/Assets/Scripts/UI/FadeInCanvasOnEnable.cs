using UnityEngine;
using DG.Tweening;

public class FadeInCanvasOnEnable : MonoBehaviour
{

    [SerializeField] private CanvasGroup _canvasGroup;

    [SerializeField] private float animationTime = 0.2f;

    private void OnEnable()
    {
        _canvasGroup.alpha = 0;
        Tween myTween = _canvasGroup.DOFade(1, animationTime);
        myTween.SetUpdate(UpdateType.Normal, true);
    }
    

}
