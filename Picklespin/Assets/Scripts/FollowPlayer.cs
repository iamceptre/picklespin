using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    private Transform followTransform;

    private void Start()
    {
        followTransform = CachedCameraMain.instance.cachedTransform;
    }
    void LateUpdate()
    {
        transform.position = followTransform.position;
    }
}
