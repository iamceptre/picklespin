using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance { get; private set; }

    [Header("Character Controller Setup")]
    public CharacterController characterController;
    [SerializeField] private Transform forwardPointer;

    [Header("Movement Speeds")]
    public float walkSpeed = 5;
    public float runSpeed = 13;
    public float crouchSpeed = 3;
    public float jumpPower = 6.5f;
    public float speedMultiplier = 1;
    [SerializeField] private float moveSmoothTime = 0.1f;

    [Header("Character Sizing & Gravity")]
    public float defaultHeight = 2;
    public float crouchHeight = 1.618f;
    [SerializeField] private float gravity = 9.81f;
    private float startingGravity;
    private readonly float stairGravity = 4000;

    [Header("Stamina & Fatigue")]
    [Range(0, 100)][HideInInspector] public float stamina = 100;
    public float fatigability = 32;

    [Header("Bhop Settings")]
    [SerializeField] private float bhopTimingThreshold = 0.15f;
    [SerializeField] private float bhopSpeedBonus = 0.4f;
    public bool canBhop = false;

    [Header("State & Movement")]
    [Range(0, 2)] public int movementStateForFMOD = 1;
    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool anyMovementKeysPressed;
    [HideInInspector] public float externalPushForce = 1;
    public Vector3 moveDirection = Vector3.zero;

    [Header("Air Control")]
    [SerializeField, Range(0, 10)] private float airControl = 0.5f;
    [SerializeField, Range(0, 10)] private float dampeningFactor = 0.1f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference jumpAction;
    private Vector2 smoothedMovement;
    private Vector2 movementVelocity;

    [Header("References")]
    [SerializeField] private FootstepSystem footstepSystem;
    [SerializeField] private CharacterControllerVelocity speedometer;
    [SerializeField] private CameraBob cameraBob;
    [SerializeField] private BarLightsAnimation barLightsAnimation;
    [SerializeField] private CameraShakeManagerV2 camShakeManager;
    [SerializeField] private StaminaBarDisplay staminaBarDisplay;

    private WaitForSeconds jumpBufferWait = new(0.15f);
    private bool jumpBuffered;

    private enum MovementState
    {
        Sneak = 0,
        Walk = 1,
        Run = 2
    }

    private MovementState currentState = MovementState.Walk;

    void Awake()
    {
        if (instance && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        startingGravity = gravity;
        currentState = MovementState.Walk;
        movementStateForFMOD = 1;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 1);
    }

    void Update()
    {
        Vector2 rawMovement = moveAction.action.ReadValue<Vector2>();
        anyMovementKeysPressed = rawMovement != Vector2.zero;
        float chosenSmoothTime = rawMovement.sqrMagnitude < smoothedMovement.sqrMagnitude ? moveSmoothTime * 1.618f : moveSmoothTime;
        smoothedMovement = Vector2.SmoothDamp(smoothedMovement, rawMovement, ref movementVelocity, chosenSmoothTime);
        HandleMovementState();
        if (!canBhop && !characterController.isGrounded)
        {
            canBhop = true;
            Invoke(nameof(ResetCanBhop), bhopTimingThreshold);
        }
        if (jumpAction.action.triggered) StartCoroutine(JumpBuffer());
        HandleMovement();
    }

    void HandleMovementState() //Update: trigger it only before footstep, not on update
    {
        bool isCrouchHeld = crouchAction.action.IsPressed();
        bool canRunCheck = characterController.isGrounded && stamina > 0 && runAction.action.IsPressed() && anyMovementKeysPressed && !isCrouchHeld;
        MovementState newState;
        if (isCrouchHeld) newState = MovementState.Sneak;
        else if (canRunCheck) newState = MovementState.Run;
        else newState = MovementState.Walk;
        if (currentState != newState)
        {
            currentState = newState;
            movementStateForFMOD = (int)newState;
            //Debug.Log("Movement State: " + movementStateForFMOD);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", movementStateForFMOD);
            isRunning = (newState == MovementState.Run);
        }
        switch (currentState)
        {
            case MovementState.Sneak:
                characterController.height = crouchHeight;
                walkSpeed = crouchSpeed;
                runSpeed = crouchSpeed;
                break;
            case MovementState.Run:
                characterController.height = defaultHeight;
                walkSpeed = 5;
                runSpeed = 13;
                StaminaDeplete();
                break;
            default:
                characterController.height = defaultHeight;
                walkSpeed = 5;
                runSpeed = 13;
                StaminaRecovery();
                break;
        }
    }

    void HandleMovement()
    {
        Vector3 forward = forwardPointer.forward;
        Vector3 right = forwardPointer.right;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        float inputX = smoothedMovement.y * currentSpeed;
        float inputY = smoothedMovement.x * currentSpeed;
        Vector3 inputDirection = (forward * inputX + right * inputY) * speedMultiplier;
        if (characterController.isGrounded)
        {
            moveDirection = inputDirection;
            moveDirection.y = -gravity * Time.deltaTime;
            if (jumpBuffered)
            {
                jumpBuffered = false;
                Jump();
            }
        }
        else
        {
            Vector3 horizontalVelocity = new Vector3(moveDirection.x, 0, moveDirection.z);
            Vector3 airControlVelocity = airControl * Time.deltaTime * inputDirection;
            horizontalVelocity += airControlVelocity;
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, dampeningFactor * Time.deltaTime);
            float maxSpeed = Mathf.Max(walkSpeed, runSpeed);
            if (horizontalVelocity.magnitude > maxSpeed) horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            moveDirection = new Vector3(horizontalVelocity.x, moveDirection.y - gravity * Time.deltaTime, horizontalVelocity.z);
        }
        if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && moveDirection.y > 0) moveDirection.y = 0;
        float preservedY = moveDirection.y;
        Vector3 adjustedDirection = moveDirection * externalPushForce;
        adjustedDirection.y = preservedY;
        characterController.Move(adjustedDirection * Time.deltaTime);
    }

    void Jump()
    {
        cameraBob.ResetBobbing();
        footstepSystem.SendJumpSignal();
        NormalGravity();
        camShakeManager.ShakeSelected(9);
        if (canBhop) BhopJump();
        else
        {
            float cost = Mathf.Clamp((1 + speedometer.horizontalVelocity) * 0.05f * fatigability, 10, 100);
            stamina -= cost;
            jumpPushForward(0.04f);
            moveDirection.y = jumpPower;
            footstepSystem.StopAllCoroutines();
        }
    }

    void BhopJump()
    {
        float cost = Mathf.Clamp((1 + speedometer.horizontalVelocity) * 0.05f * fatigability, 10, 100) * 0.3f;
        stamina -= cost * 0.5f;
        Vector3 horizontalBoost = moveDirection.normalized * bhopSpeedBonus;
        moveDirection += new Vector3(horizontalBoost.x, 0, horizontalBoost.z);
        jumpPushForward(0.23f);
        moveDirection.y = jumpPower;
        footstepSystem.StopAllCoroutines();
        footstepSystem.SendJumpSignal();
    }

    void jumpPushForward(float force)
    {
        externalPushForce = 1f;
        if (anyMovementKeysPressed && stamina > 2f)
        {
            externalPushForce = force + speedometer.horizontalVelocity * 0.1f;
            StopAllCoroutines();
            StartCoroutine(ExternalPushForceDamp());
        }
        else externalPushForce = 1f;
    }

    IEnumerator ExternalPushForceDamp()
    {
        while (externalPushForce > 1f)
        {
            externalPushForce -= Time.deltaTime * 0.3f;
            externalPushForce = Mathf.Clamp(externalPushForce, 1f, 7f);
            yield return null;
        }
    }

    IEnumerator JumpBuffer()
    {
        jumpBuffered = true;
        yield return jumpBufferWait;
        jumpBuffered = false;
    }

    public void AddExplosionJump(float explosionForce, Vector3 explosionCenter, float rangeRadius)
    {
        float distance = Vector3.Distance(transform.position, explosionCenter);
        float forceFactor = Mathf.Clamp01(1 - distance / rangeRadius);
        Vector3 pushDir = (transform.position - explosionCenter);
        pushDir.y = 1f;
        pushDir.Normalize();
        moveDirection += pushDir * (explosionForce * forceFactor);
        footstepSystem.SendJumpSignal();
        camShakeManager.ShakeSelected(9);
    }

    public void SlowMeDown()
    {
        currentState = MovementState.Sneak;
        movementStateForFMOD = 0;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 0);
        isRunning = false;
        speedMultiplier = 0.5f;
    }

    public void SpeedMeBackUp()
    {
        speedMultiplier = 1f;
    }

    void StaminaDeplete()
    {
        stamina = Mathf.Clamp(stamina - Time.deltaTime * fatigability, 0, 100);
        if (stamina <= 0)
        {
            currentState = MovementState.Walk;
            movementStateForFMOD = 1;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", 1);
            isRunning = false;
        }
        staminaBarDisplay.Refresh(false);
    }

    void StaminaRecovery()
    {
        if (characterController.isGrounded)
        {
            float rate = anyMovementKeysPressed ? 10f : 20f;
            stamina = Mathf.Clamp(stamina + Time.deltaTime * rate, 0, 100);
        }
        staminaBarDisplay.Refresh(false);
    }

    void ResetCanBhop()
    {
        canBhop = false;
    }

    public void NormalGravity()
    {
        gravity = startingGravity;
    }

    public void StairGravity()
    {
        if (characterController.isGrounded) gravity = stairGravity;
        else StartCoroutine(WaitTillLandingToSetGravity());
    }

    IEnumerator WaitTillLandingToSetGravity()
    {
        while (!characterController.isGrounded) yield return null;
        gravity = stairGravity;
    }

    public void GiveStaminaToPlayer(int howMuchStaminaIGive, bool isSilent = false)
    {
        float target = stamina + howMuchStaminaIGive;
        bool maxed = target >= 100;
        stamina = maxed ? 100 : target;
        staminaBarDisplay.Refresh(false);
        if (!isSilent) barLightsAnimation.PlaySelectedBarAnimation(1, howMuchStaminaIGive, maxed);
    }
}
