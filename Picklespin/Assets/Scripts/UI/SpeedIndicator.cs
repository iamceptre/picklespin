using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedIndicator : MonoBehaviour
{
    [Header("References (any can be left empty)")]
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

        float targetT = Mathf.InverseLerp(movement.walkSpeed, movement.MaxHorizontalSpeed, movement.DamageSpeed);

        displayedT = Mathf.Lerp(displayedT, targetT, 1f - Mathf.Exp(-sliderSmoothing * Time.deltaTime));
        Color color = Color.Lerp(slowColor, fastColor, displayedT);

        if (slider)
        {
            slider.value = displayedT * 100f;
            if (sliderFill) sliderFill.color = color;
        }

        if (Time.unscaledTime < nextRefreshTime) return;
        nextRefreshTime = Time.unscaledTime + refreshInterval;

        if (damageMultiplierText)
        {
            float multiplier = Mathf.Round(movement.SpeedDamageMultiplier * 20f) * 0.05f;
            if (!Mathf.Approximately(multiplier, lastShownMultiplier))
            {
                lastShownMultiplier = multiplier;
                damageMultiplierText.text = $"×{multiplier:0.00}";
            }
            damageMultiplierText.color = color;
        }
    }
}
