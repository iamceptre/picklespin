using UnityEngine;

public class HandSwayTempSolution : MonoBehaviour
{
    public Transform mainCamera;
    [SerializeField] private float damping = 30;

    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, mainCamera.rotation, Time.deltaTime * damping); //Clamp it so the hand wont get extreme offset when fast mouse move
        transform.position = Vector3.Lerp(transform.position, mainCamera.position, Time.deltaTime * damping * 3);
    }
}
