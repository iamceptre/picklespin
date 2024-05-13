using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ImageFadePulse : MonoBehaviour
{
    private Image myImage;

    [SerializeField] private bool startFromTransparent;
    [Range(0,1)] [SerializeField] private float fadeInOpacity = 1;
    [SerializeField] private float animationTime = 1;

    private void Awake()
    {
        myImage = GetComponent<Image>();
    }
    void Start()
    {
        if (startFromTransparent)
        {
            myImage.color = new Color(1, 1, 1, 0);
            myImage.DOFade(fadeInOpacity, animationTime).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            myImage.DOFade(myImage.color.a - fadeInOpacity, animationTime).SetLoops(-1, LoopType.Yoyo);
        }
    }


}
