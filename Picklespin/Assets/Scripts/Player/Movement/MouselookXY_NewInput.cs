using UnityEngine;
using UnityEngine.InputSystem;

public class MouselookXY : MonoBehaviour
{
    private float rotY;
    private float rotX;

    public float sensitivity = 3;
    private float startSensitivity;

    public Transform body;
    public Transform mainCamera;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Start()
    {
        startSensitivity = sensitivity;
    }

    private void Update()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            rotX += lookInput.x * sensitivity;
            rotY += lookInput.y * sensitivity;
        }
        else
        {
            rotX += lookInput.x * sensitivity * Time.deltaTime;
            rotY += lookInput.y * sensitivity * Time.deltaTime;
        }

        rotY = Mathf.Clamp(rotY, -90f, 90f);

        Quaternion bodyRotation = Quaternion.Euler(0f, rotX, 0f);
        Quaternion cameraRotation = Quaternion.Euler(-rotY, 0f, 0f);

        body.rotation = bodyRotation;
        mainCamera.localRotation = cameraRotation;
    }

    public void ZeroSensitivity()
    {
        sensitivity = 0;
    }

    public void RestoreSensitivity()
    {
        sensitivity = startSensitivity;
    }
}