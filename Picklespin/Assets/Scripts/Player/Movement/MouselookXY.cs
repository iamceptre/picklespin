using UnityEngine;
using UnityEngine.InputSystem;

public class MouselookXY : MonoBehaviour
{
    public static MouselookXY instance { get; private set; }
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private Transform body;
    [SerializeField] private Transform mainCamera;
     public float sensitivity = 3 * 0.05f;

    float rotY;
    float rotX;
    float startSensitivity;

    void Awake()
    {
        if (instance && instance != this) Destroy(this);
        else instance = this;
    }

    void OnEnable() => lookAction.action.Enable();
    void OnDisable() => lookAction.action.Disable();

    void Start()
    {
        sensitivity = PlayerPrefs.GetFloat("MouseSensitivity") * 0.06f * 0.05f;
        startSensitivity = sensitivity;
        rotY = 0f;
        rotX = 0f;
    }

    void Update()
    {
        Vector2 lookValue = lookAction.action.ReadValue<Vector2>() * sensitivity;
        rotX += lookValue.x;
        rotY = Mathf.Clamp(rotY - lookValue.y, -90f, 90f);
        mainCamera.localRotation = Quaternion.Euler(rotY, rotX, 0f);
        body.rotation = Quaternion.Euler(0f, rotX, 0f);
    }

    public void ZeroSensitivity() => sensitivity = 0;
    public void RestoreSensitivity() => sensitivity = startSensitivity;
}
