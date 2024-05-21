using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class KeyHasBeenSpawned : MonoBehaviour
{
    public static KeyHasBeenSpawned instance;

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
        myText.enabled = false;
    }


    public void Animate()
    {
        myText.enabled = true;
        myText.DOKill();
        myText.DOFade(0, 0);
        myRectTransform.localScale = Vector3.one;
        StartCoroutine(WaitAndAnimate());
    }

    private IEnumerator WaitAndAnimate()
    {
        yield return new WaitForSeconds(0.2f);
        myText.DOFade(1, 0.32f).SetEase(Ease.InSine).OnComplete(FadeOut);
        myRectTransform.DOScale(1.1f, 3.2f);
    }

    private void FadeOut()
    {
        myText.DOFade(0, 3.2f).SetEase(Ease.InSine).OnComplete(() =>
        {
            myText.DOKill();
            myRectTransform.DOKill();
           myText.enabled=false;
        });
    }


}
