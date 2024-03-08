using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform body;
    [SerializeField] private FootstepSystem footstepSystem;
    [SerializeField] private StaminaBarDisplay staminaBarDisplay;

    public float walkSpeed;
    public float runSpeed;
    public float jumpPower;
    private float gravity = 10;
    public float defaultHeight;
    public float crouchHeight;
    public float crouchSpeed;
    [Range(0, 100)] public float stamina = 100;
    public float fatigability = 32; //lower the fatigability to sprint for longer

    [SerializeField]private Vector3 moveDirection = Vector3.zero;
    public CharacterController characterController;

    private bool canMove = true;

    public bool isRunning;

    private bool anyMovementKeysPressed;

    public float externalPushForce = 1; //1 means no difference at all


    private void Start()
    {
        footstepSystem = GetComponent<FootstepSystem>();
    }

    void Update()
    {

        Vector3 forward = body.TransformDirection(Vector3.forward);
        Vector3 right = mainCamera.TransformDirection(Vector3.right);

        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            anyMovementKeysPressed = true;
        }
        else
        {
            anyMovementKeysPressed = false;
        }


            //SPRINTING LOGIC
            if (characterController.isGrounded && stamina >= 0 && Input.GetKey(KeyCode.LeftShift))
        {
            if (anyMovementKeysPressed)
            {
                isRunning = true;
                StaminaDeplete();
            }
            else
            {
                isRunning = false;
                StaminaRecovery();
            }
        }
        else
        {
            isRunning = false;
            StaminaRecovery();
        }



        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX * externalPushForce) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            Jump();
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


    private void StaminaDeplete()
    {
        stamina -= Time.deltaTime * fatigability;
        stamina = Mathf.Clamp(stamina, 0, 100);
        if (stamina <= 0)
        {
            isRunning = false;
        }
        staminaBarDisplay.RefreshBarDisplay();
    }

    private void StaminaRecovery()
    {
        if (characterController.isGrounded)
        {
            if (anyMovementKeysPressed)                                                  //if player is moving then the stamina recovery is slower 

            {
                stamina += Time.deltaTime * 8;
            }
            else
            {
                stamina += Time.deltaTime * 16;
            }
        }

        stamina = Mathf.Clamp(stamina, 0, 100);
        staminaBarDisplay.RefreshBarDisplay();
    }


    private IEnumerator JumpStaminaSmoothDeplete()
    {
        for (int i = 0; i < fatigability*0.5f; i++)
        {
            stamina--;
            yield return null;
        }
    }

    private void Jump()
    {
        jumpPushForward();
        moveDirection.y = jumpPower;
        footstepSystem.footstepSpaceCooldown = 0;                                               //makes the footstep space consistent when we land
        footstepSystem.StartCoroutine(footstepSystem.SendJumpSignal());
        StartCoroutine(JumpStaminaSmoothDeplete());
    }

    private void jumpPushForward()
    {
        if (anyMovementKeysPressed && stamina>15)
        {
            externalPushForce = 0.5f + footstepSystem.overallSpeed*0.12f;
            //StartCoroutine(ExternalPushForceDamp());
        }
        else
        {
            externalPushForce = 1;
        }

    }

    private IEnumerator ExternalPushForceDamp() //broken, after every jump the dampening gets faster wtf
    {
        while (externalPushForce>=1)
        {
            externalPushForce -= Time.deltaTime;
            externalPushForce = Mathf.Clamp(externalPushForce, 1, 5);
            yield return new WaitForEndOfFrame();
        }
    }

}