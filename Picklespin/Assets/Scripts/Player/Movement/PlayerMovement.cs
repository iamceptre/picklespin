
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public Transform hand;
    public Transform mainCamera;
    public Transform body;
    private Vector3 velocity = Vector3.zero;
    public float smoothTime = 0.3F;

    public float walkSpeed;
    public float runSpeed;
    public float jumpPower;
    private float gravity = 10;
    public float defaultHeight;
    public float crouchHeight;
    public float crouchSpeed;

    private Vector3 moveDirection = Vector3.zero;
    public CharacterController characterController;

    private bool canMove = true;

    [HideInInspector] public bool isRunning;

    private FootstepSystem footstepSystem;


    private void Start()
    {
        footstepSystem = GetComponent<FootstepSystem>();
    }



    void Update()
    {
        hand.position = Vector3.SmoothDamp(hand.position, characterController.transform.position, ref velocity, smoothTime * Time.deltaTime * 100); //fix jitter

        Vector3 forward = body.TransformDirection(Vector3.forward);
        Vector3 right = mainCamera.TransformDirection(Vector3.right);

        if (characterController.isGrounded)
        {
            isRunning = Input.GetKey(KeyCode.LeftShift);
        }

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
            footstepSystem.StartCoroutine(footstepSystem.SendJumpSignal());
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.C) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

}