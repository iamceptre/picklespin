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

    private float speedMultiplier = 1;

    private BarLightsAnimation barLightsAnimation;

    public float jumpPower;
    private float gravity = 10;
    public float defaultHeight;
    public float crouchHeight;
    [HideInInspector] [Range(0, 100)] public float stamina = 100;
    private Vector3 moveDirection = Vector3.zero;
    public CharacterController characterController;
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
        barLightsAnimation = BarLightsAnimation.instance;
        movementStateForFMOD = 1;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 1);
        footstepSystem.fixedFootstepSpace = 0.6f;//walk speed
        SetCameraBobSpeed();

    }

    public void SlowMeDown()
    {
        SetSneakSingleTick();
        speedMultiplier = 0.5f;
    }

    public void SpeedMeBackUp()
    {
        SetWalkSingleTick();
        speedMultiplier = 1;
    }

    private void SetWalkSingleTick()
    {
        if (movementStateForFMOD != 1 && !Input.GetKey(KeyCode.C) && anyMovementKeysPressed && speedMultiplier == 1)
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
        if (movementStateForFMOD != 2 && stamina > 5 && anyMovementKeysPressed && speedMultiplier == 1)
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
        if (anyMovementKeysPressed)
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

        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            anyMovementKeysPressed = true;
        }
        else
        {
            anyMovementKeysPressed = false;
        }


        if (characterController.isGrounded && stamina >= 0 && Input.GetKey(KeyCode.LeftShift))
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



        Vector3 forward = body.TransformDirection(Vector3.forward);
        Vector3 right = mainCamera.TransformDirection(Vector3.right);

        float curSpeedX = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal");
        float movementDirectionY = moveDirection.y;
        moveDirection = ((forward * curSpeedX * externalPushForce) + (right * curSpeedY)) * speedMultiplier;

        if (Input.GetButton("Jump") && characterController.isGrounded)
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

        if (Input.GetKeyDown(KeyCode.C))
        {
            SetSneakSingleTick();
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }

        if (Input.GetKeyUp(KeyCode.C))
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
        staminaBarDisplay.Refresh(false);
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
        staminaBarDisplay.Refresh(false);
    }


    private void Jump()
    {
        //footstepSystem.footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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
            footstepSystem.SendJumpSignal();
        }
    }

    private void BhopJump()
    {
        //maybe a gui that tells you that you bhopped
        stamina -= Mathf.Clamp((1 + speedometer.horizontalVelocity) * 0.05f * fatigability, 10, 100) * 0.3f; //eats less stamina than usual jump
        jumpPushForwardBhop();
        moveDirection.y = jumpPower;
        footstepSystem.StopAllCoroutines();
        footstepSystem.SendJumpSignal();
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
            externalPushForce = 0.5f + speedometer.horizontalVelocity * 0.25f; //succesful bhop push
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


    public void GiveStaminaToPlayer(int howMuchStaminaIGive)
    {


        if (stamina + howMuchStaminaIGive <= 100)
            {
                stamina += howMuchStaminaIGive;
                barLightsAnimation.PlaySelectedBarAnimation(1, howMuchStaminaIGive, false); //hp = 0, stamina = 1, mana = 2
            }
            else
            {
                stamina = 100;
                barLightsAnimation.PlaySelectedBarAnimation(1, howMuchStaminaIGive, true); //hp = 0, stamina = 1, mana = 2
            }

        staminaBarDisplay.Refresh(false);

        //RuntimeManager.PlayOneShot(manaAqquiredSound);
    }

}