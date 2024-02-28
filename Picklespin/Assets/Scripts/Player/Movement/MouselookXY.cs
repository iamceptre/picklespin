using UnityEngine;

public class MouselookXY : MonoBehaviour
{
    Vector2 rotation = Vector2.zero;
    public float sensitivity = 3;

    public Transform body;
    public Transform mainCamera;


    void Update()         //Rotation Action
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x -= Input.GetAxis("Mouse Y");

        body.rotation = Quaternion.Euler(0, rotation.y * sensitivity, 0);
        mainCamera.rotation = Quaternion.Euler(Mathf.Clamp(rotation.x, -89/sensitivity, 89/sensitivity) * sensitivity, rotation.y * sensitivity, 0); //Fix Clamp being stuck 
    }
}