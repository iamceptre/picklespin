using UnityEngine;

public class DrawGizmoSphere : MonoBehaviour
{
   [SerializeField] private float radius;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
