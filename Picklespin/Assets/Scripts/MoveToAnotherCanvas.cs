using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class MoveToAnotherCanvas : MonoBehaviour
{

    [Tooltip("Fade out those")] private Canvas canvasToGoFrom;
    [Tooltip("Fade in those")][SerializeField] private Canvas canvasToGoTo;
    private CanvasGroup canvasGroupFrom;
    private CanvasGroup canvasGroupTo;

    private void Awake()
    {
        canvasToGoFrom = GetComponentInParent<Canvas>();
        canvasGroupFrom = canvasToGoFrom.gameObject.GetComponent<CanvasGroup>();
        canvasGroupTo = canvasToGoTo.gameObject.GetComponent<CanvasGroup>();
    }

    public void Do()
    {
        if (canvasGroupFrom == null)
        {
            canvasGroupFrom.interactable = false;
            canvasToGoFrom.enabled = false;
            canvasToGoFrom.gameObject.SetActive(false);
            Debug.Log("no canvas group on selected FromCanvas, skipping the aniomation...");
            Do2();
            return;
        }


        RuntimeManager.PlayOneShot("event:/UI/UI_WHISPER");


        canvasGroupFrom.alpha = 1;
        canvasGroupFrom.DOKill();

        canvasGroupTo.DOKill();
        canvasGroupTo.alpha = 0;


        canvasGroupFrom.DOFade(0, 0.05f).OnComplete(() =>
        {

            canvasToGoFrom.enabled = false;
            canvasToGoFrom.gameObject.SetActive(false);
            Do2();
        });

    }

    private void Do2()
    {
        canvasToGoTo.gameObject.SetActive(true);
        canvasToGoTo.enabled = true;

        if (canvasGroupTo != null)
        {
            canvasGroupTo.interactable = true;
            canvasGroupTo.DOFade(1, 0.08f).OnComplete(() =>
            {
                canvasGroupTo.alpha = 1;
            });
        }

    }
}