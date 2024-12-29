using UnityEngine;

public class SpellDecalDissolve : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    // Fade settings assigned at runtime by manager
    private float fadeDelay;
    private float fadeDuration;

    private float fadeStartTime;
    private bool isFading;

    // Cache the callback to return to pool
    private System.Action<SpellDecalDissolve> onFadeComplete;

    // Reusable MaterialPropertyBlock for controlling alpha
    private static MaterialPropertyBlock _mpb;

    private void Awake()
    {
        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// Called by the SpellDecalManager when spawned.
    /// </summary>
    public void Initialize(float delay, float duration, System.Action<SpellDecalDissolve> fadeCompleteCallback)
    {
        fadeDelay = delay;
        fadeDuration = duration;
        onFadeComplete = fadeCompleteCallback;

        // Reset alpha to fully opaque
        SetAlpha(1f);

        // Mark the time we became active
        fadeStartTime = Time.time + fadeDelay;
        isFading = false;
    }

    private void Update()
    {
        // Wait until it's time to fade
        if (!isFading && Time.time >= fadeStartTime)
        {
            isFading = true;
            fadeStartTime = Time.time; // reset so we measure fade from this moment
        }

        if (isFading)
        {
            float elapsed = Time.time - fadeStartTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Lerp alpha down from 1 to 0
            float currentAlpha = Mathf.Lerp(1f, 0f, t);
            SetAlpha(currentAlpha);

            // Once fully faded, return to pool
            if (t >= 1f)
            {
                isFading = false;
                onFadeComplete?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Sets alpha using a MaterialPropertyBlock so we don't create unique materials.
    /// </summary>
    private void SetAlpha(float alpha)
    {
        // We read the current color from the sprite so that we keep the same RGB, only alter A
        Color c = spriteRenderer.color;
        c.a = alpha;

        // Update using MaterialPropertyBlock to avoid material instancing
        spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", c);
        spriteRenderer.SetPropertyBlock(_mpb);
    }
}