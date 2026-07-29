using UnityEngine;
using UnityEngine.InputSystem;

// Quake-style velocity physics on a CharacterController:
// ground friction + wish-direction acceleration, air-strafing, bhop.
// Tuning constants derive from φ (golden ratio) for a coherent, natural feel.
public class PlayerMovement : MonoBehaviour
{
    public const float PHI = PhiMath.PHI;

    public static PlayerMovement Instance { get; private set; }

    [Header("Character Controller Setup")]
    public CharacterController characterController;
    [SerializeField] private Transform forwardPointer;

    [Header("Movement Speeds")]
    public float walkSpeed = 5;
    public float runSpeed = 13;
    public float crouchSpeed = 3;
    public float jumpPower = 6.5f;
    public float speedMultiplier = 1;

    [Header("Quake Physics (φ-tuned)")]
    [SerializeField, Tooltip("how fast you reach wish speed; φ⁵ ≈ 11.09 → snappy")]
    private float groundAcceleration = 11.09f;
    [SerializeField, Tooltip("ground drag; lower = icier; φ√φ ≈ 2.058 keeps momentum well")]
    private float groundFriction = 2.058f;
    [SerializeField, Tooltip("below this speed friction bites fully and you come to rest (φ)")]
    private float frictionStopSpeed = PHI;
    [SerializeField, Tooltip("acceleration while airborne; φ⁴ ≈ 6.854")]
    private float airAcceleration = 6.854f;
    [SerializeField, Tooltip("quake air-strafe wish cap (φ); small cap + high accel = strafe gains")]
    private float airSpeedCap = PHI;

    [Header("Character Sizing & Gravity")]
    public float defaultHeight = 2;
    public float crouchHeight = 1.618f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Ground & Slope Handling")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField, Tooltip("how far below the feet ground still counts as walkable (stair steps, slope descents)")]
    private float groundSnapDistance = 0.35f;
    [SerializeField, Tooltip("constant velocity pressing the controller into walkable ground")]
    private float groundStickForce = 3f;
    [SerializeField, Tooltip("stick force used inside StairGravity trigger zones")]
    private float stairStickForce = 12f;
    [SerializeField, Tooltip("slide speed on ground steeper than the controller's slope limit")]
    private float steepSlopeSlideSpeed = 5f;

    [Header("Stamina & Fatigue")]
    [Range(0, 100)] public float stamina = 100;
    public float fatigability = 32;

    [Header("Speed Damage")]
    [SerializeField, Tooltip("damage multiplier when standing or slow-walking")]
    private float minDamageMultiplier = 0.25f;
    [SerializeField, Tooltip("damage multiplier at max speed (bhop chains, rocket jumps)")]
    private float maxDamageMultiplier = 2.5f;

    [Header("Bhop Settings")]
    [SerializeField, Tooltip("grace period after landing where a jump keeps momentum; holding jump auto-hops; 1/φ³ ≈ 0.236s")]
    private float bhopTimingThreshold = 0.236f;
    [SerializeField, Tooltip("horizontal speed multiplier gained per chained hop; φ-1 ≈ 0.618")]
    private float bhopSpeedBonus = 0.4f;

    [Header("State & Movement")]
    [Range(0, 2)] public int movementStateForFMOD = 1;
    public bool anyMovementKeysPressed;
    public Vector3 moveDirection = Vector3.zero;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("References")]
    [SerializeField] private FootstepSystem footstepSystem;
    [SerializeField] private CameraBob cameraBob;
    [SerializeField] private BarLightsAnimation barLightsAnimation;
    [SerializeField] private CameraShakeManagerV2 camShakeManager;
    [SerializeField] private StaminaBarDisplay staminaBarDisplay;

    private enum MovementState { Sneak = 0, Walk = 1, Run = 2 }
    private MovementState currentState = MovementState.Walk;

    private float defaultStickForce;
    private Vector3 groundNormal = Vector3.up;
    private bool isGroundedStable;
    private bool onWalkableGround;
    private bool airborneByImpulse;
    private float landedTime = -10f;
    private float jumpQueuedUntil = -10f;
    private Vector2 rawInput;

    // read-only state for other systems (bob, footsteps, speedometer, damage, speed GUI)
    public bool IsGroundedStable => isGroundedStable;
    public Vector3 MeasuredVelocity { get; private set; }
    public float HorizontalSpeed { get; private set; }
    public float SpeedDamageMultiplier { get; private set; } = 1f;

    public float MaxHorizontalSpeed => runSpeed * PHI;
    private bool CanBhop => Time.time - landedTime <= bhopTimingThreshold;
    private float CurrentWishSpeed => currentState switch
    {
        MovementState.Sneak => crouchSpeed,
        MovementState.Run => runSpeed,
        _ => walkSpeed
    };

    void Awake()
    {
        if (Instance && Instance != this) Destroy(this);
        else Instance = this;
    }

    void Start()
    {
        defaultStickForce = groundStickForce;
        SetFmodMovementState(MovementState.Walk);
    }

    void Update()
    {
        ProbeGround();
        rawInput = moveAction.action.ReadValue<Vector2>();
        anyMovementKeysPressed = rawInput != Vector2.zero;
        if (jumpAction.action.triggered) jumpQueuedUntil = Time.time + PHI * 0.1f;

        HandleMovementState();
        HandleMovement();
    }

    // ---------- ground probing ----------

    private bool CastToGround(out RaycastHit hit)
    {
        float radius = characterController.radius;
        Vector3 bottomSphereCenter = transform.TransformPoint(characterController.center);
        bottomSphereCenter.y -= characterController.height * 0.5f - radius;

        const float castStartOffset = 0.05f;
        Vector3 origin = bottomSphereCenter + Vector3.up * castStartOffset;
        float castDistance = castStartOffset + characterController.skinWidth + groundSnapDistance;

        if (Physics.SphereCast(origin, radius * 0.95f, Vector3.down, out hit, castDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            hit.distance = Mathf.Max(0f, hit.distance - castStartOffset);
            return true;
        }
        return false;
    }

    void ProbeGround()
    {
        bool wasStable = isGroundedStable;
        bool touchingGround;
        float groundAngle;
        if (CastToGround(out RaycastHit hit))
        {
            groundNormal = hit.normal;
            groundAngle = Vector3.Angle(groundNormal, Vector3.up);

            // spherecast edge normals can read steeper than the real surface (stair
            // lips, slope feet) and wedge the player into the slide branch — verify
            // a steep reading with a pinpoint raycast before trusting it
            if (groundAngle > characterController.slopeLimit &&
                Physics.Raycast(hit.point + Vector3.up * 0.1f, Vector3.down, out RaycastHit precise, 0.3f, groundLayers, QueryTriggerInteraction.Ignore))
            {
                float preciseAngle = Vector3.Angle(precise.normal, Vector3.up);
                if (preciseAngle < groundAngle)
                {
                    groundNormal = precise.normal;
                    groundAngle = preciseAngle;
                }
            }

            touchingGround = characterController.isGrounded || hit.distance <= characterController.skinWidth + 0.02f;
        }
        else
        {
            groundNormal = Vector3.up;
            groundAngle = 0f;
            touchingGround = characterController.isGrounded;
        }

        // a jump/explosion keeps us "airborne" until we actually come back down;
        // walking uphill (positive projected y) must NOT count as airborne
        if (airborneByImpulse && touchingGround && moveDirection.y <= 0.01f) airborneByImpulse = false;
        isGroundedStable = touchingGround && !airborneByImpulse;
        onWalkableGround = isGroundedStable && groundAngle <= characterController.slopeLimit + 0.01f;

        if (isGroundedStable && !wasStable) landedTime = Time.time;
    }

    // ---------- quake physics core ----------

    void HandleMovement()
    {
        float dt = Time.deltaTime;
        Vector3 wishVelocity = (forwardPointer.forward * rawInput.y + forwardPointer.right * rawInput.x)
                               * (CurrentWishSpeed * speedMultiplier);
        float wishSpeed = wishVelocity.magnitude;
        Vector3 wishDir = wishSpeed > 0.001f ? wishVelocity / wishSpeed : Vector3.zero;

        if (onWalkableGround)
        {
            // velocity lives in the surface plane so slopes are hugged, not stair-stepped
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal);
            if (wishSpeed > 0.001f)
                wishDir = Vector3.ProjectOnPlane(wishDir, groundNormal).normalized;

            if (Time.time <= jumpQueuedUntil || jumpAction.action.IsPressed())
            {
                jumpQueuedUntil = 0f;
                Jump(); // before friction: a hop on the landing frame keeps all momentum
            }
            else
            {
                ApplyFriction(dt);
                Accelerate(wishDir, wishSpeed, groundAcceleration, dt);
            }
        }
        else if (isGroundedStable)
        {
            // steeper than the slope limit: slide down — but jumping stays allowed,
            // so the player can always rescue themselves if geometry blocks the slide
            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
            moveDirection = slideDirection * steepSlopeSlideSpeed
                          + Vector3.ProjectOnPlane(wishVelocity, groundNormal) * 0.5f;

            if (Time.time <= jumpQueuedUntil || jumpAction.action.IsPressed())
            {
                jumpQueuedUntil = 0f;
                Jump();
            }
        }
        else
        {
            // quake air control: tiny wish cap + high acceleration = strafe gains
            wishDir.y = 0;
            if (wishDir.sqrMagnitude > 0.0001f) wishDir.Normalize();
            Accelerate(wishDir, Mathf.Min(wishSpeed, airSpeedCap), airAcceleration, dt);
            moveDirection.y -= gravity * dt;
            ClampHorizontalSpeed(MaxHorizontalSpeed);
        }

        if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && moveDirection.y > 0)
            moveDirection.y = 0;

        // grounded is false on the jump frame (airborneByImpulse) so stick/snap can't eat the jump.
        // the stick presses along the surface normal: keeps contact without slowing tangential movement
        bool grounded = isGroundedStable && !airborneByImpulse;
        characterController.Move((grounded ? moveDirection - groundNormal * groundStickForce : moveDirection) * dt);

        // sample BEFORE the snap move: CharacterController.velocity only reflects the
        // last Move call, and the vertical-only snap would zero the reported speed
        MeasuredVelocity = characterController.velocity;
        HorizontalSpeed = Mathf.Sqrt(MeasuredVelocity.x * MeasuredVelocity.x + MeasuredVelocity.z * MeasuredVelocity.z);
        SpeedDamageMultiplier = Mathf.Lerp(minDamageMultiplier, maxDamageMultiplier,
            Mathf.InverseLerp(walkSpeed, MaxHorizontalSpeed*1.5f, HorizontalSpeed));

        if (grounded && onWalkableGround) SnapToGround();
    }

    void ApplyFriction(float dt)
    {
        float speed = moveDirection.magnitude;
        if (speed < 0.01f)
        {
            moveDirection = Vector3.zero;
            return;
        }
        float control = Mathf.Max(speed, frictionStopSpeed);
        float newSpeed = Mathf.Max(speed - control * groundFriction * dt, 0f);
        moveDirection *= newSpeed / speed;
    }

    void Accelerate(Vector3 wishDir, float wishSpeed, float acceleration, float dt)
    {
        if (wishSpeed <= 0.001f) return;
        float addSpeed = wishSpeed - Vector3.Dot(moveDirection, wishDir);
        if (addSpeed <= 0f) return;
        moveDirection += wishDir * Mathf.Min(acceleration * wishSpeed * dt, addSpeed);
    }

    void ClampHorizontalSpeed(float max)
    {
        float horizontal = Mathf.Sqrt(moveDirection.x * moveDirection.x + moveDirection.z * moveDirection.z);
        if (horizontal <= max) return;
        float scale = max / horizontal;
        moveDirection.x *= scale;
        moveDirection.z *= scale;
    }

    void SnapToGround()
    {
        // keeps the controller welded to the surface over stair edges and slope crests
        if (characterController.isGrounded) return;
        if (CastToGround(out RaycastHit hit) && Vector3.Angle(hit.normal, Vector3.up) <= characterController.slopeLimit + 0.01f)
        {
            characterController.Move(Vector3.down * (hit.distance + characterController.skinWidth));
        }
    }

    // ---------- jumping ----------

    void Jump()
    {
        cameraBob.ResetBobbing();
        footstepSystem.SendJumpSignal();
        NormalGravity();
        camShakeManager.ShakeSelected(9);
        airborneByImpulse = true;

        float staminaCostScale = 1f;
        if (CanBhop && anyMovementKeysPressed)
        {
            // chained hop: momentum untouched by friction, plus a stacking bonus
            moveDirection.x *= 1f + bhopSpeedBonus;
            moveDirection.z *= 1f + bhopSpeedBonus;
            ClampHorizontalSpeed(MaxHorizontalSpeed);
            staminaCostScale = 1f / PhiMath.PHI4; // ≈ 0.146
        }

        float cost = Mathf.Clamp((1 + HorizontalSpeed) * 0.05f * fatigability, 10, 100) * staminaCostScale;
        stamina = Mathf.Max(stamina - cost, 0);

        moveDirection.y = jumpPower;
    }

    // ---------- movement state / crouch / sprint ----------

    void HandleMovementState()
    {
        bool crouchHeld = crouchAction.action.IsPressed();
        bool running = isGroundedStable && stamina > 0 && runAction.action.IsPressed() && anyMovementKeysPressed && !crouchHeld;

        MovementState newState = crouchHeld ? MovementState.Sneak : running ? MovementState.Run : MovementState.Walk;
        if (newState != currentState)
        {
            currentState = newState;
            characterController.height = newState == MovementState.Sneak ? crouchHeight : defaultHeight;
            SetFmodMovementState(newState);
        }

        if (currentState == MovementState.Run) StaminaDeplete();
        else if (currentState == MovementState.Walk) StaminaRecovery();
    }

    void SetFmodMovementState(MovementState state)
    {
        movementStateForFMOD = (int)state;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MovementState", movementStateForFMOD);
    }

    void StaminaDeplete()
    {
        stamina = Mathf.Max(stamina - Time.deltaTime * fatigability, 0);
        staminaBarDisplay.Refresh(false);
    }

    void StaminaRecovery()
    {
        if (isGroundedStable)
        {
            float rate = anyMovementKeysPressed ? 10f : 20f;
            stamina = Mathf.Min(stamina + Time.deltaTime * rate, 100);
        }
        staminaBarDisplay.Refresh(false);
    }

    // ---------- external API ----------

    public void AddExplosionJump(float explosionForce, Vector3 explosionCenter, float rangeRadius)
    {
        float forceFactor = Mathf.Clamp01(1 - Vector3.Distance(transform.position, explosionCenter) / rangeRadius);
        Vector3 pushDir = transform.position - explosionCenter;
        pushDir.y = 1f;
        pushDir.Normalize();
        moveDirection += pushDir * (explosionForce * forceFactor);
        if (moveDirection.y > 0f) airborneByImpulse = true;
        footstepSystem.SendJumpSignal();
        camShakeManager.ShakeSelected(9);
    }

    public void SlowMeDown()
    {
        currentState = MovementState.Sneak;
        SetFmodMovementState(MovementState.Sneak);
        speedMultiplier = 0.5f;
    }

    public void SpeedMeBackUp()
    {
        speedMultiplier = 1f;
    }

    public void NormalGravity()
    {
        groundStickForce = defaultStickForce;
    }

    public void StairGravity()
    {
        groundStickForce = stairStickForce;
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
