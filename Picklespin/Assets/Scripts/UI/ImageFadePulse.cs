using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ImageFadePulse : MonoBehaviour
{
    private Image myImage;
    private Color startColor;

    [SerializeField] private bool startFromTransparent;
    [Range(0, 1)][SerializeField] private float fadeInOpacity = 1;
    [SerializeField] private float animationTime = 1;

    private void Awake()
    {
        myImage = GetComponent<Image>();
        startColor = myImage.color;
    }
    void Start()
    {
        if (startFromTransparent)
        {
            myImage.color = new Color(myImage.color.r, myImage.color.g, myImage.color.b, 0);
            Tween myTween = myImage.DOFade(fadeInOpacity, animationTime).SetLoops(-1, LoopType.Yoyo);
            myTween.SetUpdate(UpdateType.Normal, true);
        }
        else
        {
            Tween myTween = myImage.DOFade(myImage.color.a - fadeInOpacity, animationTime).SetLoops(-1, LoopType.Yoyo);
            myTween.SetUpdate(UpdateType.Normal, true);
        }
    }


}
