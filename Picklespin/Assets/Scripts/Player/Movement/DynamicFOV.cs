using UnityEngine;

public class DynamicFOV : MonoBehaviour
{
    public static DynamicFOV instance { get; private set; }

    private CharacterControllerVelocity speedometer;
    private Camera mainCam;
    //[SerializeField] Camera overlayCam; //commed out because the arrow is kinda weird looking with it, need to do it in UI tho
    private float startingFOV;

    private float smoothDampVelocity;

    [SerializeField] private float intensitivity = 0.3f;
    [Tooltip("less is sharper")]
    [SerializeField] private float smoothness = 0.04f;

    // comfort multiplier for the speed-driven FOV punch, 0 = locked to base FOV
    private float speedFovStrength = 1f;

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
        mainCam = Camera.main;
    }

    void Start()
    {
        speedometer = CharacterControllerVelocity.instance;
        startingFOV = PlayerPrefs.HasKey("BaseFOV") ? PlayerPrefs.GetFloat("BaseFOV") : mainCam.fieldOfView;

        // shared "camera motion" comfort key — same slider that scales CameraBob and
        // CameraSkewController; "CameraBobStrenght" (sic) matches the options scene's slider
        if (PlayerPrefs.HasKey("CameraBobStrenght"))
        {
            SetSpeedFovStrength(PlayerPrefs.GetFloat("CameraBobStrenght") * 0.01f);
        }
    }

    // base for the speed-driven FOV; Update eases toward it, so live changes from
    // the options slider blend in smoothly
    public void SetBaseFOV(float fov)
    {
        startingFOV = fov;
    }

    public void SetSpeedFovStrength(float normalized)
    {
        speedFovStrength = Mathf.Clamp01(normalized);
    }

    void Update()
    {
        float clampedVelocity = Mathf.Clamp(speedometer.horizontalVelocity, 4, 30) - 4;

        float desiredFOV = startingFOV + (clampedVelocity * intensitivity * speedFovStrength);

        if (!Mathf.Approximately(mainCam.fieldOfView, desiredFOV))
        {
            float finalFov = Mathf.SmoothDamp(mainCam.fieldOfView, desiredFOV, ref smoothDampVelocity, smoothness);
            mainCam.fieldOfView = finalFov;
        }
    }
}
