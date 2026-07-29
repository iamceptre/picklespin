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
        // PlayerMovement samples velocity before its ground-snap Move; reading
        // characterController.velocity directly would see only the vertical snap
        PlayerMovement playerMovement = PlayerMovement.Instance;
        Vector3 velocity = playerMovement != null ? playerMovement.MeasuredVelocity : characterController.velocity;
        horizontalVelocity = new Vector3(velocity.x, 0, velocity.z).magnitude;
        verticalVelocity = Mathf.Abs(velocity.y);
    }
}