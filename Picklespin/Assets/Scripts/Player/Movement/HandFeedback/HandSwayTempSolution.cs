using UnityEngine;

public class HandSwayTempSolution : MonoBehaviour
{
    public Transform mainCamera;
    public Transform body;
    [SerializeField] private float damping = 30;

    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, mainCamera.rotation, Time.deltaTime * damping); //Clamp it so the hand wont get extreme offset when fast mouse move
        transform.position = Vector3.Lerp(transform.position, body.position + new Vector3(0, 1, 0), Time.deltaTime * damping * 3);
       //transform.position = mainCamera.position;
    }
}
