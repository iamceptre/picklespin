using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TipDisplay))]
public class HeadshotLesson : MonoBehaviour
{
    private const float Tau = 6.28f;

    private static readonly List<HeadshotMarker> markers = new();
    private static HeadshotLesson instance;
    private static bool teaching;
    private static bool taught;

    [Tooltip("how fast the marker glow churns while the tip is up")]
    [SerializeField] private float pulsesPerSecond = 2f;
    [Tooltip("the colour a marker reaches at the top of a pulse - its alpha is the peak alpha")]
    [SerializeField] private Color peakColor = Color.red;
    [Tooltip("the alpha a marker falls back to at the bottom of a pulse")]
    [SerializeField, Range(0f, 1f)] private float troughAlpha;

    private TipDisplay tip;
    private Color pulse;
    private float angularSpeed;
    private float phase;
    private float time;

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
        time = 0f;
        phase = Random.Range(0f, Tau);
        angularSpeed = pulsesPerSecond * Tau;
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
        time += Time.deltaTime * angularSpeed;

        float waveA = Mathf.Sin(time + phase);
        float waveB = Mathf.Sin(time * PhiMath.PHI);
        float waveC = Mathf.Sin(time * PhiMath.PHI4 + phase);
        float wave = 0.5f + 0.2f * (waveA + waveB) + 0.1f * waveB * waveC;

        pulse.a = Mathf.Lerp(troughAlpha, peakColor.a, wave);

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
