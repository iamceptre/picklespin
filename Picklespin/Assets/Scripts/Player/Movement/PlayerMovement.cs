using System.Collections;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{

    [HideInInspector] public static PlayerMovement instance { get; private set; } 


    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform body;
    [SerializeField] private FootstepSystem footstepSystem;
    [SerializeField] private StaminaBarDisplay staminaBarDisplay;

    public float walkSpeed;
    public float runSpeed;
    public float crouchSpeed;

    private bool isSlowedDown = false;

    public float jumpPower;
    private float gravity = 10;
    public float defaultHeight;
    public float crouchHeight;
    [HideInInspector] [Range(0, 100)] public float stamina = 100;
    private Vector3 moveDirection = Vector3.zero;
    public CharacterController characterController;
    private bool canMove = true;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool anyMovementKeysPressed;
    [HideInInspector] public float externalPushForce = 1; //1 means no difference at all
    [Range(0,2)] public int movementStateForFMOD = 1; // 0-stealth, 1-walk, 2-sprint

    [Header("Stats")]
    public float fatigability = 32; //lower the fatigability to sprint for longer

    private void Awake() //SINGLETON ! :D
    {
        if (instance != null && instance != this )
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

    }

    private void Start()
    {
        footstepSystem = GetComponent<FootstepSystem>();
    }

    public void SlowMeDown()
    {
        isSlowedDown = true;
    }

    public void SpeedMeBackUp()
    {
        isSlowedDown = false;
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


            
            if (characterController.isGrounded && stamina >= 0 && Input.GetKey(KeyCode.LeftShift) && !isSlowedDown)
        {
            if (anyMovementKeysPressed && !Input.GetKey(KeyCode.C))
            {
                movementStateForFMOD = 2;
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

        if (Input.GetKey(KeyCode.C) && canMove || isSlowedDown)
        {
            movementStateForFMOD = 0;
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            if (!isRunning) {
                movementStateForFMOD = 1;
            }
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
                stamina += Time.deltaTime * 16;
            }
            else
            {
                stamina += Time.deltaTime * 32;
            }
        }

        stamina = Mathf.Clamp(stamina, 0, 100);
        staminaBarDisplay.RefreshBarDisplay();
    }


    private void Jump()
    {
        stamina -= Mathf.Clamp((1 + footstepSystem.overallSpeed) * 0.05f * fatigability,10,100);
        jumpPushForward();
        moveDirection.y = jumpPower;
        footstepSystem.StopAllCoroutines();
        footstepSystem.StartCoroutine(footstepSystem.SendJumpSignal());
    }

    private void jumpPushForward()
    {
        if (anyMovementKeysPressed && stamina>2)
        {
            externalPushForce = 0.5f + footstepSystem.overallSpeed * 0.2f;
            StopAllCoroutines();
            StartCoroutine(ExternalPushForceDamp());
        }
        else
        {
            externalPushForce = 1;
        }

    }

    private IEnumerator ExternalPushForceDamp()
    {
        while (externalPushForce >= 1)
        {
            externalPushForce -= Time.deltaTime;
            externalPushForce = Mathf.Clamp(externalPushForce, 1, 5);
            yield return null;
        }
    }

}