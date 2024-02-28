using UnityEngine;

public class StickCameraToBody : MonoBehaviour
{
    [SerializeField] private Transform body;

    private void Update()
    {
        transform.position = body.position + new Vector3(0,1,0);
    }


}
