using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BarEase : MonoBehaviour
{
    public Slider sliderToFollow;
    [SerializeField] private Image easeFill;
    [SerializeField, Tooltip("how long the shadow holds before it starts catching up")]
    private float easeDelay = 0.5f;
    [SerializeField, Tooltip("how long the catch-up slide takes, at a constant rate")]
    private float easeDuration = 0.5f;

    private Slider me;
    private float targetValue;
    private bool behind;
    private float holdUntilTime;
    private float easeSpeed;
    private bool fillVisible;
    private float hideFillTime;

    private void Awake()
    {
        me = GetComponent<Slider>();
        fillVisible = easeFill.enabled;
    }

    private void Start() => ResetEase();

    private void Update()
    {
        float currentValue = sliderToFollow.value;

        if (currentValue >= me.value)
        {
            CatchUp(currentValue);
            if (Time.time >= hideFillTime) SetFillVisible(false);
            return;
        }

        targetValue = currentValue;

        if (!behind)
        {
            behind = true;
            holdUntilTime = Time.time + easeDelay;
            SetFillVisible(true);
        }

        if (Time.time < holdUntilTime) return;

        float duration = Mathf.Max(easeDuration, 0.0001f);
        float gap = me.value - targetValue;
        if (easeSpeed <= 0f || gap > easeSpeed * duration) easeSpeed = gap / duration;

        me.value = Mathf.MoveTowards(me.value, targetValue, easeSpeed * Time.deltaTime);

        if (me.value <= targetValue) CatchUp(targetValue);
    }

    private void CatchUp(float value)
    {
        me.value = value;
        targetValue = value;
        if (behind) hideFillTime = Time.time + easeDelay;
        behind = false;
        easeSpeed = 0f;
    }

    private void SetFillVisible(bool visible)
    {
        if (fillVisible == visible) return;
        fillVisible = visible;
        easeFill.enabled = visible;
    }

    public void ResetEase()
    {
        enabled = true;
        holdUntilTime = 0f;
        CatchUp(sliderToFollow.value);
        SetFillVisible(false);
    }
}
