using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PlayerBarDisplay : MonoBehaviour
{
    [Header("For Mana Bar: ")]
    [SerializeField] private Ammo ammo;

    [Header("For HP Bar: ")]
    [SerializeField] private PlayerHP playerHP;

    [Header("For Stamina Bar: ")]
    [SerializeField] private PlayerMovement playerMovement;

    private Slider slider;
    private float targetValue;
    private float smoothVelocity;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        RefreshBarValue();
    }

    /// <summary>
    /// Refresh the bar value, optionally with a smooth animation.
    /// </summary>
    /// <param name="smooth">If true, smoothly animate the slider value.</param>
    public void Refresh(bool smooth)
    {
        if (smooth)
            RefreshBarValueSmooth();
        else
            RefreshBarValue();
    }

    public void RefreshBarValue()
    {
        CalculateTargetValue();
        slider.value = targetValue;
    }

    public void RefreshBarValueSmooth()
    {
        CalculateTargetValue();
        StopAllCoroutines();
        StartCoroutine(SmoothValueUpdate());
    }
    private void CalculateTargetValue()
    {
        if (ammo != null)
        {
            targetValue = (float)ammo.ammo / ammo.maxAmmo * slider.maxValue;
        }
        else if (playerHP != null)
        {
            targetValue = (float)playerHP.hp / playerHP.maxHp * slider.maxValue;
        }
        else if (playerMovement != null)
        {
            targetValue = Mathf.Clamp(playerMovement.stamina, 0, slider.maxValue);
        }
    }

    private IEnumerator SmoothValueUpdate()
    {
        while (Mathf.Abs(slider.value - targetValue) > 0.01f)
        {
            slider.value = Mathf.SmoothDamp(slider.value, targetValue, ref smoothVelocity, 0.3f);
            yield return null;
        }
        slider.value = targetValue;
    }
}