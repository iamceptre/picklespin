using UnityEngine;

public class LookAtY : MonoBehaviour
{
    public Transform whatToLookAt;

    private void Start()
    {
        if (whatToLookAt == null)
        {
            whatToLookAt = CachedCameraMain.instance.cachedTransform;
        }
    }

    void Update()
    {
        Vector3 direction = whatToLookAt.position - transform.position;
        direction.y = 0;

        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }
}