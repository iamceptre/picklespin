using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;

public class TextFadeOutAndDestoryOnAwake : MonoBehaviour
{
    private TMP_Text me;
    [SerializeField] float fadeOutDuration = 0.2f;
    [SerializeField] float fadeInDuration = 0.2f;
    [SerializeField] float holdTime;

    private void Awake()
    {
        me = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        FadeIn(); 
    }

    private void FadeIn()
    {
        me.DOKill();
        me.color = new Color(me.color.r, me.color.g, me.color.b, 0);
        me.DOFade(1, fadeInDuration).OnComplete(() =>
        {
            StartCoroutine(WaitAndFadeOut(holdTime));
        });
    }

    private void FadeOut()
    {
        me.DOFade(0, fadeOutDuration).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }


    private IEnumerator WaitAndFadeOut(float time)
    {
        yield return new WaitForSeconds(time);
        FadeOut();
    }


}
