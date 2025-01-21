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
    //private bool easeFillVisible;

    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    private void Start()
    {
        me.value = sliderToFollow.value;
        targetValue = me.value;
        easeFill.enabled = true;
        //SetEaseFillAlpha(1f);
         me.value = sliderToFollow.value;
    }

    private void Update()
    {
        float currentValue = sliderToFollow.value;

        if (currentValue > me.value)
        {
            me.value = currentValue;
            targetValue = currentValue;
            easeFill.enabled = false;
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
                easeFill.enabled = false;
            }
        }
    }

    private void StartEase()
    {
        isEasing = true;
        easeSpeed = Mathf.Abs(me.value - targetValue) / easeDuration;
        easeFill.enabled = true;
    }

    /*

    public void SetEaseFillState(bool visible)
    {
        if (easeFillVisible == visible) return;
        easeFillVisible = visible;
        SetEaseFillAlpha(visible ? 1f : 0f);
    }

    private void SetEaseFillAlpha(float alpha)
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
    */
}
