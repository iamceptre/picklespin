using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class AiHealthUiBar : MonoBehaviour
{
    [SerializeField] private AiHealth aiHealth;
    [SerializeField] private Slider slider;
    //[SerializeField] private Image fillImage;
    //[SerializeField] private Image bgImage;
    //[SerializeField] private GameObject wholeCanvas;
    //[SerializeField] private BarEase barEase;
    [SerializeField] private CanvasFader canvasFader;

    private static readonly WaitForSeconds waitBeforeFadeOutTime = new(5);
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (!aiHealth) gameObject.TryGetComponent(out aiHealth);

        //fillImage.enabled = false;
        //bgImage.enabled = false;
        //wholeCanvas.SetActive(false);
    }

    public void RefreshBar()
    {
        if (aiHealth.hp != slider.value)
        {
            slider.value = aiHealth.hp;
        }

        FadeIn();
    }

    public void FadeOut()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        //bgImage.DOKill();
        //fillImage.DOKill();

        //barEase.SetEaseFillState(false, 0.15f);

        canvasFader.FadeOut();

        if (slider.value <= 0)
        {
            Destroy(gameObject);
        }

        //fillImage.DOFade(0, 0.5f);
        //bgImage.DOFade(0, 0.5f).OnComplete(() =>
        //{
        //    fillImage.enabled = false;
        //    bgImage.enabled = false;
        //    wholeCanvas.SetActive(false);


        //});
    }

    private void FadeIn()
    {
        if (slider.value > 0)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(WaitAndFadeOut());

            //wholeCanvas.SetActive(true);
            //fillImage.enabled = true;
            //bgImage.enabled = true;

            //fillImage.DOKill();
            //bgImage.DOKill();

            //fillImage.DOFade(1, 0f);
            //bgImage.DOFade(1, 0f);

            //barEase.SetEaseFillState(true, 0.15f);
            canvasFader.FadeIn();
        }
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return waitBeforeFadeOutTime;
        //FadeOut();
        canvasFader.FadeOut();
    }

    public void Detach()
    {
        Vector3 lastPosition = transform.position;
        transform.SetParent(null);
        transform.position = lastPosition;
    }
}
