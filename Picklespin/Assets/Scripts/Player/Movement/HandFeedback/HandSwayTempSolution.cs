using UnityEngine;

public class HandSwayTempSolution : MonoBehaviour
{
    public Transform mainCamera;
    public Transform body;
    [SerializeField] private float damping = 30;
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, mainCamera.rotation, Time.deltaTime * damping); //Clamp the max offset distance it so the hand wont get extreme offset when fast mouse move
        transform.position = Vector3.SmoothDamp(transform.position, body.position + new Vector3(0, 1, 0), ref velocity, 0.035f);
    }
}
