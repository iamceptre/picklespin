using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BarEase : MonoBehaviour
{
    public Slider sliderToFollow;
    [SerializeField] private Image easeFill;
    [SerializeField] private float easeDuration = 0.5f;

    private Slider me;
    private float targetValue;
    private float easeSpeed;
    private bool isEasing;
    private bool easeFillVisible;
    private float fadeSpeed = 2f;

    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    private void Start()
    {
        me.value = sliderToFollow.value;
        targetValue = me.value;

        if (easeFill != null)
        {
            easeFill.enabled = true;
            SetEaseFillAlpha(1f);
        }
    }

    private void Update()
    {
        float currentValue = sliderToFollow.value;

        // Snap to target immediately for increases
        if (currentValue > me.value)
        {
            me.value = currentValue;
            targetValue = currentValue;
            if (easeFill != null) easeFill.enabled = false;
            return;
        }
        if (!Mathf.Approximately(targetValue, currentValue))
        {
            targetValue = currentValue;
            StartEase();
        }
        if (isEasing)
        {
            float step = easeSpeed * Time.deltaTime;
            me.value = Mathf.MoveTowards(me.value, targetValue, step);

            if (Mathf.Approximately(me.value, targetValue))
            {
                isEasing = false;
                me.value = targetValue;
                if (easeFill != null) easeFill.enabled = false;
            }
        }
    }

    private void StartEase()
    {
        isEasing = true;
        easeSpeed = Mathf.Abs(me.value - targetValue) / easeDuration;
        if (easeFill != null) easeFill.enabled = true;
    }

    public void SetEaseFillState(bool visible, float fadeDuration = 0f)
    {
        if (easeFillVisible == visible) return;

        easeFillVisible = visible;

        if (fadeDuration > 0f)
        {
            fadeSpeed = 1f / fadeDuration;
        }
        else
        {
            SetEaseFillAlpha(visible ? 1f : 0f);
        }
    }

    private void SetEaseFillAlpha(float alpha)
    {
        if (easeFill != null)
        {
            Color color = easeFill.color;
            color.a = alpha;
            easeFill.color = color;

            if (!easeFillVisible && Mathf.Approximately(alpha, 0f))
            {
                easeFill.enabled = false;
            }
            else if (easeFillVisible && !easeFill.enabled)
            {
                easeFill.enabled = true;
            }
        }
    }
}
