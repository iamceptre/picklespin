using UnityEngine;
using DG.Tweening;

public class MoveToAnotherCanvas : MonoBehaviour
{

    [Tooltip("Fade out those")][SerializeField] private Canvas canvasToGoFrom;
    [Tooltip("Fade in those")][SerializeField] private Canvas canvasToGoTo;
    private CanvasGroup canvasGroupFrom;
    private CanvasGroup canvasGroupTo;

    private void Awake()
    {
        canvasGroupFrom = canvasToGoFrom.gameObject.GetComponent<CanvasGroup>();
        canvasGroupTo = canvasToGoTo.gameObject.GetComponent<CanvasGroup>();
    }

    public void Do()
    {
        if (canvasGroupFrom == null)
        {
            canvasGroupFrom.interactable = false;
            canvasToGoFrom.enabled = false;
            Debug.Log("no canvas group on selected FromCanvas, skipping the aniomation...");
            Do2();
            return;
        }


        canvasGroupFrom.alpha = 1;
        canvasGroupFrom.DOFade(0, 0.05f).OnComplete(() =>
        {
            canvasToGoFrom.enabled = false;
            Do2();
        });

    }

    private void Do2()
    {

        canvasToGoTo.enabled = true;

        if (canvasGroupTo != null)
        {
            canvasGroupTo.interactable = transform;
            canvasGroupTo.alpha = 0;
            canvasGroupTo.DOFade(1, 0.08f);
        }

    }
}