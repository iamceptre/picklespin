using UnityEngine;
using DG.Tweening;

public class FadeInCanvasGroup : MonoBehaviour
{
    private Tween myTween;
    private CanvasGroup myCanvasGroup;
    [SerializeField] private float animationTime = 0.2f;

    private void Awake()
    {
        myCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn()
    {
        //Debug.Log("fading in");
        myTween.Kill();
        myTween = myCanvasGroup.DOFade(1, animationTime);
        myTween.SetUpdate(UpdateType.Normal, true);
    }

    public void FadeOut()
    {
        //Debug.Log("fading out");
        myTween.Kill();
        myTween = myCanvasGroup.DOFade(0, animationTime * 1.6f);
        myTween.SetUpdate(UpdateType.Normal, true);
    }

}
