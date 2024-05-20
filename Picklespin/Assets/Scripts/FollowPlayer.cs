using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    void LateUpdate()
    {
        transform.position = CachedCameraMain.instance.cachedTransform.position;
    }
}
