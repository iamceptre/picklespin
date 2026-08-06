using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TipDisplay))]
public class HeadshotLesson : MonoBehaviour
{
    private const float Tau = 6.2831855f;

    private static readonly List<HeadshotMarker> markers = new();
    private static HeadshotLesson instance;
    private static bool teaching;
    private static bool taught;

    [Tooltip("how many times a second an enemy eye marker pulses while the tip is up")]
    [SerializeField] private float pulsesPerSecond = 2f;
    [Tooltip("the colour a marker reaches at the top of a pulse - its alpha is the peak alpha")]
    [SerializeField] private Color peakColor = Color.red;
    [Tooltip("the alpha a marker falls back to at the bottom of a pulse")]
    [SerializeField, Range(0f, 1f)] private float troughAlpha;

    private TipDisplay tip;
    private Color pulse;
    private float angularSpeed;
    private float halfAlphaSpan;
    private float phase;

    public static bool Teaching => teaching;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        markers.Clear();
        instance = null;
        teaching = false;
        taught = false;
    }

    public static void Register(HeadshotMarker marker)
    {
        markers.Add(marker);
        marker.SetVisible(teaching);
    }

    public static void Unregister(HeadshotMarker marker) => markers.Remove(marker);

    public static void NotifyHeadshot()
    {
        if (instance) instance.Finish();
    }

    private void Awake()
    {
        instance = this;
        tip = GetComponent<TipDisplay>();
        enabled = false;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void Begin()
    {
        if (taught || teaching) return;

        teaching = true;
        phase = 0f;
        angularSpeed = pulsesPerSecond * Tau;
        halfAlphaSpan = (peakColor.a - troughAlpha) * 0.5f;
        pulse = peakColor;
        SetMarkers(true);

        tip.ShowTip();
        enabled = true;
    }

    // Begin arrives on a delay - a headshot before it must not spend a tip nobody saw
    private void Finish()
    {
        if (!teaching) return;

        taught = true;
        teaching = false;
        enabled = false;
        SetMarkers(false);
        tip.HideTip();
    }

    private void Update()
    {
        phase += Time.deltaTime * angularSpeed;
        if (phase > Tau) phase -= Tau;
        pulse.a = troughAlpha + halfAlphaSpan * (1f + Mathf.Sin(phase));

        List<HeadshotMarker> pulsing = markers;
        int count = pulsing.Count;
        for (int i = 0; i < count; i++)
        {
            pulsing[i].Sprite.color = pulse;
        }
    }

    private static void SetMarkers(bool visible)
    {
        for (int i = 0; i < markers.Count; i++)
        {
            markers[i].SetVisible(visible);
        }
    }
}
