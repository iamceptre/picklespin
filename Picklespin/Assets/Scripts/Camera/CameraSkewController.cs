using UnityEngine;

public class CameraSkewController : MonoBehaviour
{
    public static CameraSkewController instance { get; private set; }

    private PlayerMovement playerMovement;
    [SerializeField] private float maxSkewAngle = 10f;
    [SerializeField] private float skewIntensity = 0.1f;
    [SerializeField] private float skewSmoothSpeed = 20f;

    // comfort multiplier, 0 = no tilt; shares the CameraBob/DynamicFOV "camera motion" slider
    private float strength = 1f;

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
        playerMovement = PlayerMovement.Instance;

        // "CameraBobStrenght" (sic) matches the options scene slider's settingName key
        if (PlayerPrefs.HasKey("CameraBobStrenght"))
        {
            SetStrength(PlayerPrefs.GetFloat("CameraBobStrenght") * 0.01f);
        }
    }

    public void SetStrength(float normalized)
    {
        strength = Mathf.Clamp01(normalized);
    }

    void Update()
    {
        Vector3 moveDirection = playerMovement.moveDirection;


        Vector3 skew = new Vector3(
            Mathf.Clamp(-moveDirection.z * -skewIntensity, -maxSkewAngle, maxSkewAngle),
            0,
            Mathf.Clamp(moveDirection.x * -skewIntensity, -maxSkewAngle, maxSkewAngle)
        ) * strength;

        Quaternion targetRotation = Quaternion.Euler(
            skew.x,
            0,
            skew.z
        );

        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * skewSmoothSpeed);
    }
}
