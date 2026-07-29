using UnityEngine;

public class RecoilMultiplier : MonoBehaviour
{
    public static RecoilMultiplier instance { get; private set; }
    private CharacterControllerVelocity speedometer;

    public float currentRecoil;

    [Header("Apex Window")]
    [SerializeField, Tooltip("no vertical recoil while |Y speed| is below this — the jump-apex sweet spot (φ ≈ 1.62 m/s, about a third of a second around the apex)")]
    private float apexWindow = PhiMath.PHI;
    [SerializeField, Tooltip("Y speed at which vertical recoil reaches full strength (φ⁴)")]
    private float fullRecoilSpeed = PhiMath.PHI4;
    [SerializeField, Range(0f, 1f), Tooltip("how strongly the airborne apex also cancels horizontal recoil (1 = perfect accuracy at the top of the arc)")]
    private float airborneApexBonus = 1f;

    [Header("Strength")]
    [SerializeField, Tooltip("vertical recoil strength; raised from 1.5 to φ√φ ≈ 2.06 to compensate for the apex dead zone")]
    private float jumpingRecoil = 2.058f;
    [SerializeField, Tooltip("horizontal (sprint) recoil strength")]
    private float sprintingRecoil = 0.2f;
    private float oldVert, oldHor;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        speedometer = CharacterControllerVelocity.instance;
    }

    private void Update()
    {
        float newVert = speedometer.verticalVelocity;
        float newHor = speedometer.horizontalVelocity;

        if (Mathf.Approximately(oldVert, newVert) && Mathf.Approximately(oldHor, newHor)) return;
        oldVert = newVert;
        oldHor = newHor;

        // dead zone around the jump apex, then a smoothstep ramp: shots fired near
        // the top of the arc (where Y speed flips sign) are fully accurate, and
        // recoil eases in past the window instead of growing linearly
        float t = Mathf.InverseLerp(apexWindow, fullRecoilSpeed, newVert);
        float apexCurve = t * t * (3f - 2f * t);

        float horizontalTerm = newHor * sprintingRecoil * 2f;

        // airborne apex intensifier: right where Y speed crosses zero, horizontal
        // recoil melts away too — quadratic falloff concentrates the reward at the peak
        PlayerMovement movement = PlayerMovement.Instance;
        if (movement != null && !movement.IsGroundedStable)
        {
            float apexCloseness = 1f - Mathf.InverseLerp(0f, apexWindow, newVert);
            horizontalTerm *= 1f - airborneApexBonus * apexCloseness * apexCloseness;
        }

        currentRecoil = ((apexCurve * fullRecoilSpeed * jumpingRecoil) + horizontalTerm) * 0.01f;
    }
}
