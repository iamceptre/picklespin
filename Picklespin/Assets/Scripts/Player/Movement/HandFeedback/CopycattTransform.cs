using UnityEngine;

public class CopycattTransform : MonoBehaviour
{

    public Transform whereToCopyFrom;
    void Update()
    {
        transform.rotation = whereToCopyFrom.rotation;
        transform.position = whereToCopyFrom.position;
    }
}
