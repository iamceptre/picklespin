using UnityEngine;

public class Helper_Arrow : MonoBehaviour //works only with objects that are not moving
{
    public Transform target;

    void Update()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }
}
