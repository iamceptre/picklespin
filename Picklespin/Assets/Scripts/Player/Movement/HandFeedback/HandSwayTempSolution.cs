using UnityEngine;

public class HandSwaySystematic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform body;
    private MouselookXY_old mouselook;
    private float mouseSensitivtyCached = 3;

    [Header("Rotation Sway (Mouse)")]
    [SerializeField] private float rotationSwayAmount = 5f;
    [SerializeField] private float rotationSwaySmooth = 8f;
    [SerializeField] private float maxRotationAngle = 10f;

    [Header("Position Settings")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0, 1f, 0);
    [SerializeField] private float positionSmoothTime = 0.05f;

    [Header("Movement-Based Position Sway")]
    [SerializeField] private float movementSwayAmount = 0.05f;
    [SerializeField] private float maxMovementOffset = 0.1f;

    private Quaternion _initialLocalRotation;
    private Vector3 _smoothVelocity;
    private Vector3 _lastCameraPosition;

    private void Start()
    {
        _initialLocalRotation = transform.localRotation;
        _lastCameraPosition = mainCamera.position;
        mouselook = MouselookXY_old.instance;
        mouseSensitivtyCached = mouselook.sensitivity;
    }

    private void LateUpdate()
    {
        if (Time.deltaTime <= 0)
        {
            _lastCameraPosition = mainCamera.position;
            return;
        }


        Vector3 currentCameraPos = mainCamera.position;
        Vector3 worldVelocity = (currentCameraPos - _lastCameraPosition) / Time.deltaTime;
        _lastCameraPosition = currentCameraPos;

        if (float.IsNaN(worldVelocity.x) || float.IsNaN(worldVelocity.y) || float.IsNaN(worldVelocity.z))
        {
            return;
        }


        float forwardSpeed = Vector3.Dot(worldVelocity, mainCamera.forward);
        float rightSpeed = Vector3.Dot(worldVelocity, mainCamera.right);
        float upSpeed = Vector3.Dot(worldVelocity, mainCamera.up);


        float offsetX = -rightSpeed * movementSwayAmount;
        float offsetY = -upSpeed * movementSwayAmount;
        float offsetZ = -forwardSpeed * movementSwayAmount;

        offsetX = Mathf.Clamp(offsetX, -maxMovementOffset, maxMovementOffset);
        offsetY = Mathf.Clamp(offsetY, -maxMovementOffset, maxMovementOffset);
        offsetZ = Mathf.Clamp(offsetZ, -maxMovementOffset, maxMovementOffset);
        Vector3 localOffset = new Vector3(offsetX, offsetY, offsetZ);
        Vector3 offsetWorld = mainCamera.TransformDirection(localOffset);

        Vector3 targetPosition = body.position + baseOffset + offsetWorld;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _smoothVelocity,
            positionSmoothTime
        );


        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivtyCached;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivtyCached;


        float rotX = -mouseY * rotationSwayAmount;  
        float rotY = mouseX * rotationSwayAmount;

        rotX = Mathf.Clamp(rotX, -maxRotationAngle, maxRotationAngle);
        rotY = Mathf.Clamp(rotY, -maxRotationAngle, maxRotationAngle);

        Quaternion xQuat = Quaternion.AngleAxis(rotX, Vector3.right);
        Quaternion yQuat = Quaternion.AngleAxis(rotY, Vector3.up);
        Quaternion targetRot = _initialLocalRotation * yQuat * xQuat;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * rotationSwaySmooth
        );
    }
}
