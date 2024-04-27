using UnityEngine;

public class LerpFollowVector2 : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.3f; 

    void Update()
    {
        transform.position = Vector2.Lerp(transform.position, target.position, smoothTime * Time.unscaledDeltaTime);
    }
}
