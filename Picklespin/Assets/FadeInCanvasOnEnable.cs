using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FadeInCanvasOnEnable : MonoBehaviour
{

    [SerializeField] private CanvasGroup _canvasGroup;

    [SerializeField] private float animationTime = 0.2f;

    private void Start()
    {
        _canvasGroup.DOFade(1, animationTime);
    }
    

}
