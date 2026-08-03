using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextFadeFlicker : MonoBehaviour
{

    private TMP_Text me;
    private float startingAlpha;

    [SerializeField] private bool playOnAwake = false;
    public float animationTime;


    private void Awake()
    {
        me = GetComponent<TMP_Text>();
        startingAlpha = me.color.a;

        if (playOnAwake)
        {
            StartFlicker();
        }
    }


    public void StartFlicker()
    {
        me.color = me.color.WithAlpha(0f);
        me.DOKill();
        Flicker();
    }

    public void StopFlicker()
    {
        me.DOKill();
        me.color = me.color.WithAlpha(startingAlpha);
    }

    private void Flicker()
    {
        me.DOFade(1, animationTime).SetLoops(-1, LoopType.Yoyo);
    }

    public void RestartTweening()
    {
        me.DOKill();
        me.color = me.color.WithAlpha(0f);
        Flicker();
    }


}
