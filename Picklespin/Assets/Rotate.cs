using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private float speed = 30;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    private void Update()
    {
        transform.Rotate(rotationAxis, Time.deltaTime * speed, Space.Self);
    }
}
