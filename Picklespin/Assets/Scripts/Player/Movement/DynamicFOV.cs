using UnityEngine;

public class DynamicFOV : MonoBehaviour
{
    private CharacterControllerVelocity speedometer;
    private Camera mainCam;
    //[SerializeField] Camera overlayCam; //commed out because the arrow is kinda weird looking with it, need to do it in UI tho
    private float startingFOV;

    private float smoothDampVelocity;

    [SerializeField] private float intensitivity = 0.3f;
    [Tooltip("less is sharper")]
    [SerializeField] private float smoothness = 0.04f; 

    private void Awake()
    {
        mainCam = Camera.main;
    }

    void Start()
    {
        speedometer = CharacterControllerVelocity.instance;
        startingFOV = mainCam.fieldOfView;
    }

    void Update()
    {
        float clampedVelocity = Mathf.Clamp(speedometer.horizontalVelocity, 4, 30) - 4;

        float desiredFOV = startingFOV + (clampedVelocity * intensitivity);

        if (!Mathf.Approximately(mainCam.fieldOfView, desiredFOV))
        {
            float finalFov = Mathf.SmoothDamp(mainCam.fieldOfView, desiredFOV, ref smoothDampVelocity, smoothness);
            mainCam.fieldOfView = finalFov;
            //overlayCam.fieldOfView = finalFov;
        }
    }
}
