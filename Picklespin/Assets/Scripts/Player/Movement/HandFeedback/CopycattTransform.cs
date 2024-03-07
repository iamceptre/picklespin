using UnityEngine;

public class CopycattTransform : MonoBehaviour
{

    public Transform whereToCopyFrom;
    void LateUpdate()
    {
        transform.rotation = whereToCopyFrom.rotation;
        transform.position = whereToCopyFrom.position;
    }
}
