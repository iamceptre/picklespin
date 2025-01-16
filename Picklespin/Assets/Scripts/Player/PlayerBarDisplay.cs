using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PlayerBarDisplay : MonoBehaviour
{
    [Header("Assign only ONE reference for this bar")]
    [SerializeField] private Ammo ammo;
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private PlayerMovement playerMovement;
    private const float smoothTime = 0.5f;

    private Slider slider;
    private Coroutine smoothUpdateCoroutine;
    private float velocity;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        slider.value = GetCurrentNormalizedValue() * slider.maxValue;
    }

    public void Refresh(bool smooth)
    {
        float targetValue = GetCurrentNormalizedValue() * slider.maxValue;

        if (smooth)
        {
            StopActiveCoroutine();
            smoothUpdateCoroutine = StartCoroutine(SmoothValueRoutine(targetValue));
        }
        else
        {
            StopActiveCoroutine();
            slider.value = targetValue;
        }
    }

    private float GetCurrentNormalizedValue()
    {
        return playerHP ? (float)playerHP.hp / playerHP.maxHp
             : ammo ? (float)ammo.ammo / ammo.maxAmmo
             : playerMovement ? Mathf.Clamp01(playerMovement.stamina * 0.01f)
             : 0f;
    }

    private IEnumerator SmoothValueRoutine(float targetValue)
    {
        velocity = 0f;
        float currentValue = slider.value;

        while (!Mathf.Approximately(currentValue, targetValue))
        {
            currentValue = Mathf.SmoothDamp(currentValue, targetValue, ref velocity, smoothTime);
            slider.value = currentValue;
            yield return null;
        }

        slider.value = targetValue;
        smoothUpdateCoroutine = null;
    }

    private void StopActiveCoroutine()
    {
        if (smoothUpdateCoroutine != null)
        {
            StopCoroutine(smoothUpdateCoroutine);
            smoothUpdateCoroutine = null;
        }
    }
}