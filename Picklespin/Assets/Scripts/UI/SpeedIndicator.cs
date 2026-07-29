using UnityEngine;
using UnityEngine.UI;
using TMPro;

// HUD readout of the player's horizontal speed and the speed-damage multiplier.
// Wire any subset of the references: text-only, bar-only, or both.
public class SpeedIndicator : MonoBehaviour
{
    [Header("References (any can be left empty)")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text damageMultiplierText;
    [SerializeField] private Slider slider;

    [Header("Look")]
    [SerializeField] private Color slowColor = new(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color fastColor = new(1f, 0.78f, 0.2f, 1f);
    [SerializeField] private float refreshInterval = 0.1f;
    [SerializeField, Tooltip("how quickly the bar eases toward the real speed")]
    private float sliderSmoothing = 8f;

    private PlayerMovement movement;
    private Image sliderFill;
    private float nextRefreshTime;
    private float displayedT;
    private int lastShownSpeed = -1;
    private float lastShownMultiplier = -1f;

    private void Start()
    {
        movement = PlayerMovement.Instance;
        if (slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            if (slider.fillRect) sliderFill = slider.fillRect.GetComponent<Image>();
        }
    }

    private void Update()
    {
        float targetT = Mathf.InverseLerp(movement.walkSpeed, movement.MaxHorizontalSpeed, movement.HorizontalSpeed);

        // slider eases every frame; texts refresh on the throttled interval
        displayedT = Mathf.Lerp(displayedT, targetT, 1f - Mathf.Exp(-sliderSmoothing * Time.deltaTime));
        Color color = Color.Lerp(slowColor, fastColor, displayedT);

        if (slider)
        {
            slider.value = displayedT * 100f;
            if (sliderFill) sliderFill.color = color;
        }

        if (Time.unscaledTime < nextRefreshTime) return;
        nextRefreshTime = Time.unscaledTime + refreshInterval;

        if (speedText)
        {
            int shownSpeed = Mathf.RoundToInt(movement.HorizontalSpeed);
            if (shownSpeed != lastShownSpeed)
            {
                lastShownSpeed = shownSpeed;
                speedText.text = shownSpeed.ToString();
            }
            speedText.color = color;
        }

        if (damageMultiplierText)
        {
            float multiplier = Mathf.Round(movement.SpeedDamageMultiplier * 20f) * 0.05f; // 0.05 steps
            if (!Mathf.Approximately(multiplier, lastShownMultiplier))
            {
                lastShownMultiplier = multiplier;
                damageMultiplierText.text = $"×{multiplier:0.00}";
            }
            damageMultiplierText.color = color;
        }
    }
}
