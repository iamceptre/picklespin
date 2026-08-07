using UnityEngine;
using UnityEngine.UI;

// A bar reads its pool every frame rather than waiting to be pushed at. Pushing was
// fine while one system owned one bar, but a class can fold several pools into one
// (Umbral spends the same bar for magicka, health and breath) and the pushes then
// arrive in one frame from step and continuous sources at once, each with its own
// idea of how the bar should move.
//
// Losses land the instant the pool drops - BarEase is what shows what was lost.
// Gains slide in, which is the only place easing belongs.
[RequireComponent(typeof(Slider))]
public class PlayerBarDisplay : MonoBehaviour
{
    [Header("Assign only ONE reference for this bar")]
    [SerializeField] private Ammo ammo;
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField, Tooltip("auto-found on the slider's fill if left empty - the bar pulses while its own pool is running out")]
    private PulsatingImage lowPulsation;
    [SerializeField, Tooltip("how long a gain takes to slide in; losses are always instant")]
    private float gainEaseTime = 0.5f;

    private Slider slider;
    private float velocity;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (!lowPulsation && slider.fillRect) slider.fillRect.TryGetComponent(out lowPulsation);
    }

    // a bar switched back on by PlayerClassHud must not show one frame of its old value
    private void OnEnable() => Snap();

    private void Update()
    {
        float fraction = Fraction;
        float target = fraction * slider.maxValue;

        slider.value = target < slider.value
            ? target
            : Mathf.SmoothDamp(slider.value, target, ref velocity, gainEaseTime);

        // the pool's own value, not the eased one: the warning must not lag the bar
        if (lowPulsation) lowPulsation.RefreshLowState(fraction, LowThreshold);
    }

    // The value is polled, so these only decide whether the bar may slide there. Kept
    // because the systems (and Inspector events) still call them.
    public void Refresh(bool smooth)
    {
        if (!smooth) Snap();
    }

    public void SetContinuousValue(float value, float maxValue) => Snap();

    // a bar whose class has folded it away has never woken, so it has nothing to snap and no
    // slider to snap it on - the systems behind it go on pushing at it either way
    private void Snap()
    {
        if (!slider) return;

        float fraction = Fraction;
        slider.value = fraction * slider.maxValue;
        velocity = 0f;
        if (lowPulsation) lowPulsation.RefreshLowState(fraction, LowThreshold);
    }

    private float Fraction =>
          playerHP ? playerHP.DisplayFraction
        : ammo ? ammo.DisplayFraction
        : playerMovement ? playerMovement.StaminaFraction
        : 0f;

    // read live from the system behind the bar, never copied: a wish that raises a
    // pool moves its warning with it
    private float LowThreshold =>
          playerHP ? playerHP.LowHealthThreshold
        : ammo ? Ammo.LowMagickaThreshold
        : playerMovement ? playerMovement.NoSprintThreshold
        : 0f;
}
