using UnityEngine;

public class CachedCameraMain : MonoBehaviour
{
    public static CachedCameraMain instance {get; private set;}

    public Transform cachedTransform;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
}
