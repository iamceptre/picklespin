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
    private const float smoothTime = 0.1f; //mana bar bugs out when this value is set higher pls fix it, make sure only this script interacts with the slider values

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
            if (smoothUpdateCoroutine != null)
                StopCoroutine(smoothUpdateCoroutine);

            smoothUpdateCoroutine = StartCoroutine(SmoothValueRoutine(targetValue));
        }
        else
        {
            slider.value = targetValue;
        }
    }

    private float GetCurrentNormalizedValue()
    {
        if (playerHP) return (float)playerHP.hp / playerHP.maxHp;
        if (ammo) return (float)ammo.ammo / ammo.maxAmmo;
        if (playerMovement) return Mathf.Clamp01(playerMovement.stamina * 0.01f);
        return 0f;
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
    }
}