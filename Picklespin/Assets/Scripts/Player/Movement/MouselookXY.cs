using UnityEngine;
using UnityEngine.InputSystem;

public class MouselookXY : MonoBehaviour
{
    public static MouselookXY instance { get; private set; }
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private Transform body;
    [SerializeField] private Transform mainCamera;

    private const float SliderValueToSensitivity = 0.0015f;

    public float sensitivity { get; private set; }

    float rotY;
    float rotX;

    void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        RestoreSensitivity();
    }

    void OnEnable() => lookAction.action.Enable();
    void OnDisable() => lookAction.action.Disable();

    void Update()
    {
        Vector2 lookValue = lookAction.action.ReadValue<Vector2>() * sensitivity;
        rotX += lookValue.x;
        rotY = Mathf.Clamp(rotY - lookValue.y, -90f, 90f);
        mainCamera.localRotation = Quaternion.Euler(rotY, rotX, 0f);
        body.rotation = Quaternion.Euler(0f, rotX, 0f);
    }

    public void ZeroSensitivity() => sensitivity = 0f;

    public void RestoreSensitivity() => sensitivity = PlayerPrefs.GetFloat(SettingsDefaults.MouseSensitivityKey, SettingsDefaults.MouseSensitivity) * SliderValueToSensitivity;
}
