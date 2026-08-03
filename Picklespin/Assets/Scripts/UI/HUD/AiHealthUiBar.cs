using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AiHealthUiBar : MonoBehaviour
{
    [SerializeField] private AiHealth aiHealth;
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasFader canvasFader;
    [SerializeField, Tooltip("FFDE8C - what the bar turns while this one is fighting for you")]
    private readonly Color alliedFillColor = GameColors.Highlight;

    private static readonly WaitForSeconds waitBeforeFadeOutTime = new(5);
    private Coroutine fadeCoroutine;
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private Image fillImage;
    private Color hostileFillColor;

    private void Awake()
    {
        if (!aiHealth) aiHealth = GetComponentInParent<AiHealth>(true);
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;

        if (slider && slider.fillRect) fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage) hostileFillColor = fillImage.color;
    }

    public void SetAllied(bool allied)
    {
        if (fillImage) fillImage.color = allied ? alliedFillColor : hostileFillColor;
    }

    public void RefreshBar()
    {
        if (!aiHealth) return;

        if (!Mathf.Approximately(aiHealth.hp, slider.value))
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

    }

    public void ResetBar()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        transform.SetParent(originalParent, false);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        transform.localScale = originalLocalScale;
        SetAllied(false);
    }

    private void FadeIn()
    {

        if (!gameObject.activeInHierarchy) return;

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
