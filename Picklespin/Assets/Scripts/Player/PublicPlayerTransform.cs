using UnityEngine;

public class PublicPlayerTransform : MonoBehaviour
{
    public Transform PlayerTransform;
    public static PublicPlayerTransform instance { get; private set; }

    private void Awake()
    {
        PlayerTransform = transform;

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
