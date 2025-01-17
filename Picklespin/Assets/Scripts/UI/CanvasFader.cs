using UnityEngine;
using System.Collections;

public class CanvasFader : MonoBehaviour
{
    [Header("Canvas/Fade Settings")]
    private readonly float fadeDuration = 0.5f;
    [SerializeField] private bool interactible = false;
    [SerializeField] private bool toggleCanvasGameObjectActiveState = false;

    [Header("References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    private Canvas _canvas;
    private Coroutine _currentFadeRoutine;
    private GameObject _canvasObject;

    private void Awake()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        _canvasObject = _canvasGroup.gameObject;
        _canvas = _canvasObject.GetComponent<Canvas>();
    }

    public void FadeIn() => StartFade(1f);
    public void FadeOut() => StartFade(0f);

    private void StartFade(float targetAlpha)
    {
        if (_currentFadeRoutine != null) StopCoroutine(_currentFadeRoutine);
        _currentFadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    public IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = _canvasGroup.alpha;
        float elapsedTime = 0f;

        if (targetAlpha > 0f) //when set visible
        {
            SetState(true);
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = EaseInOutSine(Mathf.Clamp01(elapsedTime / fadeDuration));
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        _canvasGroup.alpha = targetAlpha;

        if (Mathf.Approximately(targetAlpha, 0f)) //when reaches 0
        {
            SetState(false);
        }

        _currentFadeRoutine = null;
    }

    private void SetState(bool state)
    {
        if (interactible)
        {
            _canvasGroup.interactable = state;
            _canvasGroup.blocksRaycasts = state;
        }

        if (toggleCanvasGameObjectActiveState)
        {
            _canvasObject.SetActive(state);
        }

        _canvas.enabled = state;
    }

    private float EaseInOutSine(float x) => -0.5f * (Mathf.Cos(Mathf.PI * x) - 1f);



}
