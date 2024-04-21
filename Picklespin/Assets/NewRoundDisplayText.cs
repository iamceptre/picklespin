using TMPro;
using UnityEngine;
using DG.Tweening;

public class NewRoundDisplayText : MonoBehaviour
{
    public static NewRoundDisplayText instance;

    private TMP_Text myText;
    private RectTransform myRectTransform;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        myText = gameObject.GetComponent<TMP_Text>();
        myRectTransform = gameObject.GetComponent<RectTransform>();
    }

    private void Start()
    {
        myText.DOFade(0, 0);
    }

    private void DisableMe()
    {
        gameObject.SetActive(false); 
    }

    public void Animate()
    {
        myText.DOKill();
        myText.DOFade(0, 0);
        myRectTransform.localScale = Vector3.one;
        myText.DOFade(1, 0.2f).SetEase(Ease.InSine).OnComplete(FadeOut);
        myRectTransform.DOScale(1.1f, 2);
    }

    private void FadeOut()
    {
        myText.DOFade(0, 2).SetEase(Ease.InSine).OnComplete(DisableMe);
    }


}
