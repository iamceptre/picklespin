using UnityEngine;

public class MouselookXY : MonoBehaviour
{
    private float rotY;
    private float rotX;

    public float sensitivity = 3;
    private float startSensitivty;

    public Transform body;
    public Transform mainCamera;


    private void Start()
    {
        startSensitivty = sensitivity;
    }

    void Update()
    {

        rotX += Input.GetAxis("Mouse X") * sensitivity;
        rotY += Input.GetAxis("Mouse Y") * sensitivity;

        rotY = Mathf.Clamp(rotY, -90f, 90f);

        body.rotation = Quaternion.Euler(0, rotX, 0);
        mainCamera.rotation = Quaternion.Euler(-rotY, rotX, 0f);
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