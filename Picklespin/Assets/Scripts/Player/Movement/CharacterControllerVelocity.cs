using UnityEngine;

public class CharacterControllerVelocity : MonoBehaviour
{
    public static CharacterControllerVelocity instance;

    [SerializeField] private CharacterController characterController;

    public float horizontalVelocity;
    public float verticalVelocity;

    void Awake()
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

    void Update()
    {
        Vector3 velocity = characterController.velocity;
        horizontalVelocity = new Vector3(velocity.x, 0, velocity.z).magnitude;
        verticalVelocity = Mathf.Abs(velocity.y);
    }
}