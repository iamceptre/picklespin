using UnityEngine;

public class PublicPlayerTransform : MonoBehaviour
{
    public Transform PlayerTransform;
    public static PublicPlayerTransform Instance { get; private set; }

    private void Awake()
    {
        PlayerTransform = transform;

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

    }
}
