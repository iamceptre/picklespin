using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class AiHealthUiBar : MonoBehaviour
{
    [SerializeField] private AiHealth aiHealth;
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasFader canvasFader;

    private static readonly WaitForSeconds waitBeforeFadeOutTime = new(5);
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (!aiHealth) gameObject.TryGetComponent(out aiHealth);
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


        canvasFader.FadeOut();

        if (slider.value <= 0)
        {
            Destroy(gameObject);
        }

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
            canvasFader.FadeIn();
        }
    }

    private IEnumerator WaitAndFadeOut()
    {
        yield return waitBeforeFadeOutTime;
        canvasFader.FadeOut();
    }

    public void Detach()
    {
        Vector3 lastPosition = transform.position;
        transform.SetParent(null);
        transform.position = lastPosition;
    }
}
