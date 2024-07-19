using UnityEngine;

public class MouselookXY_old : MonoBehaviour
{
    public static MouselookXY_old instance { get; private set; }

    private float rotY;
    private float rotX;

    public float sensitivity = 3;
    private float startSensitivty;

    public Transform body;
    public Transform mainCamera;

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
        startSensitivty = sensitivity;
        rotY = 0f;
        rotX = 0f;
    }

        void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        rotX += mouseX;
        rotY = Mathf.Clamp(rotY - mouseY, -90f, 90f);

        mainCamera.localRotation = Quaternion.Euler(rotY, rotX, 0f);
        body.rotation = Quaternion.Euler(0f, rotX, 0f);
    }

    public void ZeroSensitivty()
    {
        sensitivity = 0;
    }

    public void RestoreSensitivity()
    {
        sensitivity = startSensitivty;
    }
}