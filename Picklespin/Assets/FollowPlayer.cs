using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Transform whoToFollow;

    void Start()
    {
        whoToFollow = Camera.main.transform; 
    }

    void LateUpdate()
    {
        transform.position = whoToFollow.position;
    }
}
