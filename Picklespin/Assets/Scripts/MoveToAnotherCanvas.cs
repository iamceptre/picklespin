using UnityEngine;
using DG.Tweening;
using FMODUnity;
using System.Collections;

public class MoveToAnotherCanvas : MonoBehaviour
{
    [Tooltip("Fade out those")]
    private Canvas canvasToGoFrom;
    [Tooltip("Fade in those")]
    [SerializeField] private Canvas canvasToGoTo;

    private CanvasGroup canvasGroupFrom;
    private CanvasGroup canvasGroupTo;

    private bool isTransitioning = false; 

    private void Awake()
    {
        canvasToGoFrom = GetComponentInParent<Canvas>();
        canvasGroupFrom = canvasToGoFrom?.gameObject.GetComponent<CanvasGroup>();
        canvasGroupTo = canvasToGoTo?.gameObject.GetComponent<CanvasGroup>();
    }

    public void Do()
    {
        if (isTransitioning) return; 
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        isTransitioning = true;

        if (canvasGroupFrom == null || canvasGroupTo == null)
        {
            Debug.LogError("CanvasGroup is missing on one of the canvases.");
            isTransitioning = false;
            yield break;
        }

        canvasGroupFrom.interactable = false;
        canvasGroupFrom.blocksRaycasts = false;

        RuntimeManager.PlayOneShot("event:/UI/UI_WHISPER");

        canvasGroupFrom.DOKill(true);
        canvasGroupTo.DOKill(true);

        canvasGroupFrom.alpha = 1;
        yield return canvasGroupFrom.DOFade(0, 0.1f).SetEase(Ease.InOutSine).WaitForCompletion();

        canvasToGoFrom.enabled = false;

        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        canvasToGoTo.enabled = true;

        canvasGroupTo.alpha = 0;
        yield return canvasGroupTo.DOFade(1, 0.16f).SetEase(Ease.InOutSine).WaitForCompletion();

        canvasGroupTo.interactable = true;
        canvasGroupTo.blocksRaycasts = true;
        isTransitioning = false;
    }
}