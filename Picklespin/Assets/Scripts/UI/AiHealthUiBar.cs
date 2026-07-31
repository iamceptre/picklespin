using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AiHealthUiBar : MonoBehaviour
{
    [SerializeField] private AiHealth aiHealth;
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasFader canvasFader;

    private static readonly WaitForSeconds waitBeforeFadeOutTime = new(5);
    private Coroutine fadeCoroutine;
    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalScale;

    private void Awake()
    {
        if (!aiHealth) aiHealth = GetComponentInParent<AiHealth>(true); // the bar hangs off a child
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;
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
        // stays alive (faded out, detached) so the pooled enemy can reclaim it via ResetBar
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
        transform.localScale = originalLocalScale; // Detach() rewrites localScale to keep world size
    }

    private void FadeIn()
    {
        // pooled resets refresh the bar while the enemy is inactive: no coroutine can run
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
