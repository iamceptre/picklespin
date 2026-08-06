using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedIndicator : MonoBehaviour
{
    [Header("References (any can be left empty)")]
    [SerializeField] private TMP_Text damageMultiplierText;
    [SerializeField] private Slider slider;

    private readonly Color slowColor = GameColors.FadedWhite;
    private readonly Color fastColor = GameColors.Critical;

    [Header("Look")]
    [SerializeField] private float refreshInterval = 0.1f;

    private PlayerMovement movement;
    private Image sliderFill;
    private float nextRefreshTime;
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
        float t = movement.SpeedDamageT;
        Color color = Color.Lerp(slowColor, fastColor, t);

        if (slider)
        {
            slider.value = t * 100f;
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
