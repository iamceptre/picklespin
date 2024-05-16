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

    /*
    void Update()
    {

        rotX += Input.GetAxis("Mouse X") * sensitivity;
        rotY += Input.GetAxis("Mouse Y") * sensitivity;

        rotY = Mathf.Clamp(rotY, -90f, 90f);

        body.rotation = Quaternion.Euler(0, rotX, 0);
        mainCamera.localRotation = Quaternion.Euler(-rotY, body.rotation.x, 0f);
    }

    */

        void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotX += mouseX * sensitivity;
        rotY += mouseY * sensitivity;

        rotY = Mathf.Clamp(rotY, -90f, 90f);

        Quaternion bodyRotation = Quaternion.Euler(0f, rotX, 0f);
        Quaternion cameraRotation = Quaternion.Euler(-rotY, 0f, 0f);

        body.rotation = bodyRotation;
        mainCamera.localRotation = cameraRotation;
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