using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PlayerBarDisplay : MonoBehaviour
{
    [SerializeField] private Ammo ammo;
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private PlayerMovement playerMovement;

    private Slider slider;
    private Coroutine smoothUpdateCoroutine;
    private const float SmoothAnimationDuration = 0.3f;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        Refresh(false);
    }

    public void Refresh(bool smooth)
    {

        float targetValue = CalculateTargetValue();

        if (smooth)
        {
            StartSmoothUpdate(targetValue);
        }
        else
        {
            slider.value = targetValue;
        }
    }

    private float CalculateTargetValue()
    {
        return ammo != null ? (float)ammo.ammo / ammo.maxAmmo * slider.maxValue :
               playerHP != null ? (float)playerHP.hp / playerHP.maxHp * slider.maxValue :
               Mathf.Clamp(playerMovement.stamina, 0, slider.maxValue);
    }

    private void StartSmoothUpdate(float targetValue)
    {
        if (smoothUpdateCoroutine != null)
        {
            StopCoroutine(smoothUpdateCoroutine);
        }

        smoothUpdateCoroutine = StartCoroutine(SmoothValueUpdate(targetValue));
    }

    private IEnumerator SmoothValueUpdate(float targetValue)
    {
        float startValue = slider.value;
        float elapsedTime = 0f;

        while (elapsedTime < SmoothAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / SmoothAnimationDuration);
            yield return null;
        }

        slider.value = targetValue;
    }
}
