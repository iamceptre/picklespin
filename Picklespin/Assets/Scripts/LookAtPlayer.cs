using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Transform player;
    [SerializeField] private float distance = 16;

    private Transform _transform;


    private void Awake()
    {
        _transform = transform;
    }

    void Start()
    {
        player = Camera.main.transform;
    }

    void Update()
    {
        if (Vector3.Distance(_transform.position, player.position) < distance)
        {
            Look();
        }
        else
        {
            enabled = false;
        }

    }


    private void Look()
    {
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.Normalize();
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 4 * Time.deltaTime);
    }

}
