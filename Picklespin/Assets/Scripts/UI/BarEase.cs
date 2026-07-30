using UnityEngine;
using UnityEngine.UI;

// The "shadow" bar that trails behind a real one, showing what was just lost.
//
// When the followed value drops, the shadow holds where it was for easeDelay so
// the lost chunk stays readable, then smoothly catches up. Gains are never eased
// — the shadow snaps straight up so it can never sit above the real fill.
//
// The hold starts when the shadow *falls behind*, not on every change. Bars that
// drop in steps (mana, enemy HP) and bars that drain continuously (stamina,
// player HP) both work: a continuous drain would otherwise re-arm the hold every
// frame and the shadow would never move at all.
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

    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    // one code path for the first spawn and every pooled respawn
    private void Start() => ResetEase();

    private void Update()
    {
        float currentValue = sliderToFollow.value;

        if (currentValue >= me.value)
        {
            CatchUp(currentValue); // gained, or already level
            return;
        }

        targetValue = currentValue;

        if (!behind)
        {
            behind = true;
            holdUntilTime = Time.time + easeDelay;
            easeFill.enabled = true;
        }

        if (Time.time < holdUntilTime) return;

        // Constant-rate catch-up, so the slide stays linear instead of crawling
        // as it closes in. The rate is latched, and only re-armed when the gap
        // grows past what it can still cover in easeDuration — that keeps a
        // continuously draining bar trailing at a steady distance.
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
        behind = false;
        easeSpeed = 0f;
        easeFill.enabled = false;
    }

    // the death event disables this component, so pooled reuse has to switch it
    // back on and re-sync it to the bar it follows
    public void ResetEase()
    {
        enabled = true;
        holdUntilTime = 0f;
        CatchUp(sliderToFollow.value);
    }
}
