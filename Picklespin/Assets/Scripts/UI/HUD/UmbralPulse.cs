using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UmbralPulse : MonoBehaviour
{
    private const float MinLitStrength = 0.55f;

    [SerializeField, Tooltip("how fast the glow churns the moment the bar crosses the line")]
    private float pulseSpeed = 6f;
    [SerializeField, Tooltip("extra churn with the bar completely full")]
    private float chargeSpeedBoost = 5f;
    [SerializeField, Tooltip("alpha at the bottom of a pulse")]
    private float minAlpha = 0.1f;
    [SerializeField, Tooltip("alpha at the top of a pulse")]
    private float maxAlpha = 0.9f;
    [SerializeField, Tooltip("how far the glow swells past its authored scale at the top of a pulse")]
    private float scaleJitter = 0.35f;
    [SerializeField, Tooltip("seconds the glow takes to come up when the bar charges, and to die when it drops back")]
    private float fadeDuration = 0.25f;
    [SerializeField, Range(0f, 1f), Tooltip("how far the peak of a pulse whites out once the bar is full - the arc-flash of an overloaded bar")]
    private float overloadPeak = 0.55f;

    private readonly Color surgeColor = GameColors.UmbralSurge;
    private readonly Color overloadColor = GameColors.UmbralOverload;

    private Image glow;
    private RectTransform rect;
    private Ammo ammo;
    private Vector3 baseScale;
    private Color glowColor;

    private float time;
    private float phase;
    private float strength;
    private float fadeRate;
    private float lastThreshold = -1f;
    private float headroomScale;
    private bool armed;

    private void Awake()
    {
        glow = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;
        glowColor = glow.color;
        phase = Random.Range(0f, Mathf.PI * 2f);
        fadeRate = fadeDuration > 0f ? 1f / fadeDuration : 1f;
        glow.enabled = false;
    }

    private void OnEnable() => PlayerClasses.Changed += Refresh;

    private void OnDisable() => PlayerClasses.Changed -= Refresh;

    private void Start()
    {
        ammo = Ammo.instance;
        Refresh();
    }

    private void Refresh()
    {
        armed = PlayerClasses.Chosen == PlayerClassId.Umbral;

        if (PlayerClassHud.Instance)
        {
            Color barColor = PlayerClassHud.Instance.BarLightColor(HudResource.Magicka);
            barColor.a = glowColor.a;
            glowColor = barColor;
        }

        if (armed) return;

        strength = 0f;
        rect.localScale = baseScale;
        glow.enabled = false;
    }

    private void Update()
    {
        if (!armed) return;

        float delta = Time.unscaledDeltaTime;
        float charge = Charge();
        float target = charge > 0f ? Mathf.Lerp(MinLitStrength, 1f, charge) : 0f;

        strength = Mathf.MoveTowards(strength, target, fadeRate * delta);

        if (strength <= 0f)
        {
            if (!glow.enabled) return;
            rect.localScale = baseScale;
            glow.enabled = false;
            return;
        }

        if (!glow.enabled) glow.enabled = true;

        time += delta * (pulseSpeed + chargeSpeedBoost * charge);

        float waveA = Mathf.Sin(time + phase);
        float waveB = Mathf.Sin(time * PhiMath.PHI);
        float waveC = Mathf.Sin(time * PhiMath.PHI4 + phase);
        float wave = 0.5f + 0.2f * (waveA + waveB) + 0.1f * waveB * waveC;

        Color color = Color.LerpUnclamped(glowColor, surgeColor, charge);
        color = Color.LerpUnclamped(color, overloadColor, wave * wave * charge * overloadPeak);
        color.a = Mathf.Lerp(minAlpha, maxAlpha, wave) * strength;
        glow.color = color;

        float swell = wave * scaleJitter * strength;
        float tremor = waveC * scaleJitter * strength * 0.4f;
        rect.localScale = new Vector3(
            baseScale.x * (1f + swell),
            baseScale.y * (1f + swell + tremor),
            baseScale.z);
    }

    private float Charge()
    {
        if (!ammo) return 0f;

        float threshold = UmbralUpgrades.ChargedBarThreshold;
        if (threshold != lastThreshold)
        {
            lastThreshold = threshold;
            headroomScale = threshold < 1f ? 1f / (1f - threshold) : 0f;
        }

        float over = ammo.Fraction - threshold;
        return over > 0f ? Mathf.Min(over * headroomScale, 1f) : 0f;
    }
}
