using UnityEngine;

public class LookAt : MonoBehaviour
{

    [SerializeField]
    [Tooltip("if null, it looks at player camera")]
    private Transform whatToLookAt;

    private void Start()
    {
        if (whatToLookAt == null)
        {
            whatToLookAt = CachedCameraMain.instance.cachedTransform;
        }
    }

    private void LateUpdate()
    {
        transform.LookAt(whatToLookAt);
    }

}
