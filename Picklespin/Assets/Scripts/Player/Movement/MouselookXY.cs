using UnityEngine;

public class MouselookXY : MonoBehaviour
{
    //Vector2 rotation = Vector2.zero;
    private float rotY;
    private float rotX;

    public float sensitivity = 3;

    public Transform body;
    public Transform mainCamera;


    void Update()         //Rotation Action
    {

        rotX += Input.GetAxis("Mouse X") * sensitivity;
        rotY += Input.GetAxis("Mouse Y") * sensitivity;

        rotY = Mathf.Clamp(rotY, -90f, 90f);

        body.rotation = Quaternion.Euler(0, rotX, 0);
        mainCamera.rotation = Quaternion.Euler(-rotY, rotX, 0f);
    }
}