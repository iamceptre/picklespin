using UnityEngine;
using DG.Tweening;
using FMODUnity;
using System.Collections;

public class MoveToAnotherCanvas : MonoBehaviour
{
    [Tooltip("Canvas to fade out, aka me")]
    [SerializeField] private Canvas canvasToGoFrom;

    [Tooltip("Canvas to fade in")]
    [SerializeField] private Canvas canvasToGoTo;

    private CanvasGroup canvasGroupFrom;
    private CanvasGroup canvasGroupTo;

    private void Awake()
    {
        canvasGroupFrom = canvasToGoFrom.GetComponent<CanvasGroup>();
        canvasGroupTo = canvasToGoTo.GetComponent<CanvasGroup>();
    }

    public void Do()
    {
        StartCoroutine(TransitionCoroutine());
    }

    private IEnumerator TransitionCoroutine()
    {
        canvasGroupFrom.interactable = false;
        canvasGroupFrom.blocksRaycasts = false;


        canvasGroupFrom.DOKill();
        canvasGroupFrom.alpha = 1;
        yield return canvasGroupFrom.DOFade(0, 0.1f).SetEase(Ease.InOutSine).WaitForCompletion();

        canvasToGoFrom.enabled = false;
        RuntimeManager.PlayOneShot("event:/UI/UI_WHISPER");

        canvasToGoTo.enabled = true;

        canvasGroupTo.DOKill();
        canvasGroupTo.alpha = 0;
        yield return canvasGroupTo.DOFade(1, 0.16f).SetEase(Ease.InOutSine).WaitForCompletion();

        canvasGroupTo.interactable = true;
        canvasGroupTo.blocksRaycasts = true;
    }
}