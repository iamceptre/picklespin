using UnityEngine;

public class EvilEntityMind : MonoBehaviour
{

    [Range(0, 100)] public float hp = 1;
    public bool isDead = false;
    private float speed = 1f;


    private GameObject player;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Update()
    {
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
    }


}
