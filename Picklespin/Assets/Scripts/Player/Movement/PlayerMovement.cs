using System.Collections;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public static PlayerMovement instance { get; private set; } 


    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform body;
    private FootstepSystem footstepSystem;
    [SerializeField] private StaminaBarDisplay staminaBarDisplay;
    [SerializeField] private Bhop bhop;
    private CameraBob cameraBob;

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

    private CharacterControllerVelocity speedometer;

    [Header("Stats")]
    public float fatigability = 32; //lower the fatigability to sprint for longer

    private void Awake()
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
        footstepSystem = FootstepSystem.instance;
        speedometer = CharacterControllerVelocity.instance;
        cameraBob = CameraBob.instance;
        movementStateForFMOD = 1;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 1);
        footstepSystem.fixedFootstepSpace = 0.6f;//walk speed
        SetCameraBobSpeed();

    }

    public void SlowMeDown()
    {
        SetSneakSingleTick();
        isSlowedDown = true;
    }

    public void SpeedMeBackUp()
    {
        SetWalkSingleTick();
        isSlowedDown = false;
    }

    private void SetWalkSingleTick()
    {
        if (movementStateForFMOD != 1 && !Input.GetKey(KeyCode.C) && anyMovementKeysPressed)
        {
            movementStateForFMOD = 1;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 1);
            footstepSystem.fixedFootstepSpace = 0.6f;//walk speed
            SetCameraBobSpeed();
            isRunning = false;
        }
    }

    private void SetRunSingleTick()
    {
        if (movementStateForFMOD != 2 && stamina > 5 && anyMovementKeysPressed)
        {
            movementStateForFMOD = 2;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 2);
            footstepSystem.fixedFootstepSpace = 0.22f; //run speed
            SetCameraBobSpeed();
            isRunning = true;
        }
    }

    private void SetSneakSingleTick()
    {
        if (movementStateForFMOD != 0 && anyMovementKeysPressed)
        {
            movementStateForFMOD = 0;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 0);
            footstepSystem.fixedFootstepSpace = 0.8f; //sneak speed
            SetCameraBobSpeed();
            isRunning = false;
        }
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
                SetRunSingleTick();
                StaminaDeplete();
            }
            else
            {
                SetWalkSingleTick();
                StaminaRecovery();
            }
        }
        else
        {
            SetWalkSingleTick();
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

            //if hit ceiling, stop jumping
            if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && moveDirection.y > 0)
            {
                    moveDirection.y = 0;
            }

        }

        if (Input.GetKeyDown(KeyCode.C) && canMove || isSlowedDown)
        {
            SetSneakSingleTick();
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }

        if (Input.GetKeyUp(KeyCode.C) && canMove || isSlowedDown)
        {
            if (!isRunning)
            {
                SetWalkSingleTick();
            }
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);


    }


    private void SetCameraBobSpeed()
    {
        if (cameraBob.bobSpeed != 0)
        {
            cameraBob.bobSpeed = 2 / (footstepSystem.fixedFootstepSpace);
        }
    }


    private void StaminaDeplete()
    {
        stamina -= Time.deltaTime * fatigability;
        stamina = Mathf.Clamp(stamina, 0, 100);
        if (stamina <= 0)
        {
            SetWalkSingleTick();
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
        footstepSystem.footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        if (bhop != null && bhop.canBhop)
        {
            BhopJump();
        }
        else
        {
            stamina -= Mathf.Clamp((1 + speedometer.horizontalVelocity) * 0.05f * fatigability, 10, 100);
            jumpPushForward();
            moveDirection.y = jumpPower;
            footstepSystem.StopAllCoroutines();
            footstepSystem.StartCoroutine(footstepSystem.SendJumpSignal());
        }
    }

    private void BhopJump()
    {
        //maybe a gui that tells you that you bhopped
        stamina -= Mathf.Clamp((1 + speedometer.horizontalVelocity) * 0.05f * fatigability, 10, 100) * 0.3f; //eats less stamina than usual jump
        jumpPushForwardBhop();
        moveDirection.y = jumpPower;
        footstepSystem.StopAllCoroutines();
        footstepSystem.StartCoroutine(footstepSystem.SendJumpSignal());
    }

    private void jumpPushForward()
    {
        if (anyMovementKeysPressed && stamina>2)
        {
            externalPushForce = 0.4f + speedometer.horizontalVelocity * 0.2f;
            StopAllCoroutines();
            StartCoroutine(ExternalPushForceDamp());
        }
        else
        {
            externalPushForce = 1;
        }

    }

    private void jumpPushForwardBhop()
    {
        if (Input.GetAxis("Horizontal") != 0)
        {
            externalPushForce = 0.5f + speedometer.horizontalVelocity * 0.25f;
            StopAllCoroutines();
            StartCoroutine(ExternalPushForceDamp());
        }
        else
        {
            if (speedometer.horizontalVelocity>8) {
                //fail bhop penalty
                externalPushForce = 0.5f + speedometer.horizontalVelocity * 0.13f;
                externalPushForce = Mathf.Clamp(externalPushForce, 1, 7);
            }
        }

    }



    private IEnumerator ExternalPushForceDamp()
    {
        while (externalPushForce >= 1)
        {
            externalPushForce -= Time.deltaTime;
            externalPushForce = Mathf.Clamp(externalPushForce, 1, 7);
            yield return null;
        }
    }

}