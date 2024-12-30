using UnityEngine;

public class SpellDecalDissolve : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float fadeDelay;
    private float fadeDuration;

    private float fadeStartTime;
    private bool isFading;

    private System.Action<SpellDecalDissolve> onFadeComplete;

    private static MaterialPropertyBlock _mpb;

    private void Awake()
    {
        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();
    }

    public void Initialize(float delay, float duration, System.Action<SpellDecalDissolve> fadeCompleteCallback)
    {
        fadeDelay = delay;
        fadeDuration = duration;
        onFadeComplete = fadeCompleteCallback;

        SetAlpha(1f);

        fadeStartTime = Time.time + fadeDelay;
        isFading = false;
    }

    private void Update()
    {
        if (!isFading && Time.time >= fadeStartTime)
        {
            isFading = true;
            fadeStartTime = Time.time;
        }

        if (isFading)
        {
            float elapsed = Time.time - fadeStartTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            float currentAlpha = Mathf.Lerp(1f, 0f, t);
            SetAlpha(currentAlpha);

            if (t >= 1f)
            {
                isFading = false;
                onFadeComplete?.Invoke(this);
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;

        spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", c);
        spriteRenderer.SetPropertyBlock(_mpb);
    }
}