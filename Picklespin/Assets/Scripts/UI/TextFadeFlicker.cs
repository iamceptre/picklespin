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
        me.color = new Color(me.color.r, me.color.g, me.color.b, 0);
        me.DOKill();
        Flicker();
    }

    public void StopFlicker()
    {
        me.DOKill();
        me.color = new Color(me.color.r, me.color.g, me.color.b, startingAlpha);
    }

    private void Flicker()
    {
        me.DOFade(1, animationTime).SetLoops(-1, LoopType.Yoyo);
    }

    public void RestartTweening()
    {
        me.DOKill();
        me.color = new Color(me.color.r, me.color.g, me.color.b, 0);
        Flicker();
    }


}
