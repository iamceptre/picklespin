using UnityEngine;

public class LookAt : MonoBehaviour
{

    [SerializeField] private Transform whatToLookAt;

    private void Start()
    {
        if (whatToLookAt == null)
        {
            //whatToLookAt = GameObject.FindGameObjectWithTag("MainCamera").transform;
            whatToLookAt = CachedCameraMain.instance.cachedTransform;
        }
    }

    private void LateUpdate()
    {
        transform.LookAt(whatToLookAt);
    }

}
