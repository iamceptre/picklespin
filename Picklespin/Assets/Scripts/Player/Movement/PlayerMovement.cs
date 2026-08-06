using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public const float PHI = PhiMath.PHI;

    public static PlayerMovement Instance { get; private set; }

    [Header("Character Controller Setup")]
    public CharacterController characterController;
    [SerializeField] private Transform forwardPointer;

    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float runSpeed = 13f;
    public float crouchSpeed = 3;
    public float jumpPower = 6.5f;
    public float speedMultiplier = 1;

    [Header("Quake Physics")]
    [SerializeField, Tooltip("how fast you reach wish speed")]
    private float groundAcceleration = 11f;
    [SerializeField, Tooltip("ground drag; lower = icier")]
    private float groundFriction = 3f;
    [SerializeField, Tooltip("below this speed friction bites fully and you come to rest")]
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

    [Header("Grappling Hook")]
    [SerializeField, Tooltip("distance from the target point that counts as arrival when the pull doesn't touch anything first")]
    private float grappleArrivalDistance = 1f;

    [Header("Stamina & Fatigue")]
    public float stamina = 100;
    [Tooltip("raised permanently by the angel's stamina wish, so nothing may assume 100")]
    public float maxStamina = 100;
    public float fatigability = 32;
    [SerializeField, Tooltip("stamina you must recover after emptying the bar before sprinting and full-power jumps come back")]
    private float staminaRecoveryThreshold = 20f;

    public bool IsExhausted { get; private set; }

    public float NoSprintThreshold => maxStamina > 0f ? staminaRecoveryThreshold / maxStamina : 0f;

    [Header("Speed Damage")]
    [SerializeField, Tooltip("damage multiplier when standing or slow-walking")]
    private float minDamageMultiplier = 0.25f;
    [SerializeField, Tooltip("damage multiplier at max speed (bhop chains, rocket jumps)")]
    private float maxDamageMultiplier = 2.5f;
    [SerializeField, Tooltip("speed at which maxDamageMultiplier is reached, as a multiple of MaxHorizontalSpeed (real speed is hard-clamped to MaxHorizontalSpeed, so keep this at 1 for the peak to be reachable)")]
    private float damageMultiplierSpeedCapScale = 1f;
    [SerializeField, Tooltip("seconds the multiplier needs to catch up while speed rises - short enough to feel immediate, long enough to smooth the jitter out of the raw speed; 1/φ⁴ ≈ 0.146s")]
    private float damageMultiplierRiseTime = 0.146f;
    [SerializeField, Tooltip("seconds the multiplier needs to decay while speed drops, so losing momentum costs damage slowly; 1/φ ≈ 0.618s")]
    private float damageMultiplierFallTime = 0.618f;

    [Header("Bhop Settings")]
    [SerializeField, Tooltip("grace period after landing where a jump keeps momentum; holding jump auto-hops; 1/φ³ ≈ 0.236s")]
    private float bhopTimingThreshold = 0.236f;
    [SerializeField, Tooltip("horizontal speed multiplier gained per chained hop; φ-1 ≈ 0.618")]
    private float bhopSpeedBonus = 0.4f;

    [Header("State & Movement")]
    [HideInInspector][Range(0, 2)] public int movementStateForFMOD = 1;
    [HideInInspector] public bool anyMovementKeysPressed;
    [HideInInspector] public Vector3 moveDirection = Vector3.zero;

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
    [SerializeField] private CameraShakeSettings jumpShake = new()
    {
        rotationAmount = new Vector3(0.2f, 0.03f, 0.03f), numberOfShakes = 2, speed = 35f, decay = 0.8f, uiShakeModifier = 0f
    };
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

    public bool IsGroundedStable => isGroundedStable;
    public bool IsRocketJumping { get; private set; }
    public bool IsGrappling => grappling;
    public Vector3 MeasuredVelocity { get; private set; }
    public float HorizontalSpeed { get; private set; }
    public float SpeedDamageMultiplier { get; private set; } = 1f;

    public float DamageSpeed { get; private set; }

    public float SpeedDamageT => Mathf.Clamp01((SpeedDamageMultiplier - minDamageMultiplier) * damageMultiplierRangeInverse);

    private float damageMultiplierVelocity;
    private float damageMultiplierRangeInverse;

    private Vector3 externalVelocity;
    private bool grappling;
    private Vector3 grappleTarget;
    private float grappleSpeed;

    public void ReportExternalVelocity(Vector3 velocity) => externalVelocity = velocity;

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

        SpeedDamageMultiplier = minDamageMultiplier;
        damageMultiplierRangeInverse = maxDamageMultiplier > minDamageMultiplier ? 1f / (maxDamageMultiplier - minDamageMultiplier) : 0f;
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

        if (airborneByImpulse && !grappling && touchingGround && moveDirection.y <= 0.01f) airborneByImpulse = false;
        isGroundedStable = touchingGround && !airborneByImpulse;
        if (isGroundedStable) IsRocketJumping = false;
        onWalkableGround = isGroundedStable && groundAngle <= characterController.slopeLimit + 0.01f;

        if (isGroundedStable && !wasStable) landedTime = Time.time;
    }

    void HandleMovement()
    {
        float dt = Time.deltaTime;

        if (grappling)
        {
            UpdateGrapplePull();
        }
        else
        {
            Vector3 wishVelocity = (forwardPointer.forward * rawInput.y + forwardPointer.right * rawInput.x)
                                   * (CurrentWishSpeed * speedMultiplier);
            float wishSpeed = wishVelocity.magnitude;
            Vector3 wishDir = wishSpeed > 0.001f ? wishVelocity / wishSpeed : Vector3.zero;

            if (onWalkableGround)
            {
                moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal);
                if (wishSpeed > 0.001f)
                    wishDir = Vector3.ProjectOnPlane(wishDir, groundNormal).normalized;

                if (Time.time <= jumpQueuedUntil || jumpAction.action.IsPressed())
                {
                    jumpQueuedUntil = 0f;
                    Jump();
                }
                else
                {
                    ApplyFriction(dt);
                    Accelerate(wishDir, wishSpeed, groundAcceleration, dt);
                }
            }
            else if (isGroundedStable)
            {
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
                wishDir.y = 0;
                if (wishDir.sqrMagnitude > 0.0001f) wishDir.Normalize();
                Accelerate(wishDir, Mathf.Min(wishSpeed, airSpeedCap), airAcceleration, dt);
                moveDirection.y -= gravity * dt;
                ClampHorizontalSpeed(MaxHorizontalSpeed);
            }
        }

        if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && moveDirection.y > 0)
            moveDirection.y = 0;

        bool grounded = isGroundedStable && !airborneByImpulse;
        characterController.Move((grounded ? moveDirection - groundNormal * groundStickForce : moveDirection) * dt);

        MeasuredVelocity = characterController.velocity;
        HorizontalSpeed = Mathf.Sqrt(MeasuredVelocity.x * MeasuredVelocity.x + MeasuredVelocity.z * MeasuredVelocity.z);

        Vector3 damageVelocity = moveDirection + externalVelocity;
        externalVelocity = Vector3.zero;
        DamageSpeed = new Vector2(damageVelocity.x, damageVelocity.z).magnitude;
        float damageMultiplierTarget = Mathf.Lerp(minDamageMultiplier, maxDamageMultiplier,
            Mathf.InverseLerp(walkSpeed, MaxHorizontalSpeed * damageMultiplierSpeedCapScale, DamageSpeed));
        float smoothTime = damageMultiplierTarget > SpeedDamageMultiplier ? damageMultiplierRiseTime : damageMultiplierFallTime;
        SpeedDamageMultiplier = Mathf.Clamp(
            Mathf.SmoothDamp(SpeedDamageMultiplier, damageMultiplierTarget, ref damageMultiplierVelocity, smoothTime, Mathf.Infinity, dt),
            minDamageMultiplier, maxDamageMultiplier);

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
        if (characterController.isGrounded) return;
        if (CastToGround(out RaycastHit hit) && Vector3.Angle(hit.normal, Vector3.up) <= characterController.slopeLimit + 0.01f)
        {
            characterController.Move(Vector3.down * (hit.distance + characterController.skinWidth));
        }
    }

    void Jump()
    {
        bool weakLegs = IsExhausted;

        cameraBob.ResetBobbing();
        footstepSystem.SendJumpSignal();
        NormalGravity();
        camShakeManager.Shake(jumpShake);
        airborneByImpulse = true;

        float staminaCostScale = 1f;
        if (CanBhop && anyMovementKeysPressed)
        {
            moveDirection.x *= 1f + bhopSpeedBonus;
            moveDirection.z *= 1f + bhopSpeedBonus;
            ClampHorizontalSpeed(MaxHorizontalSpeed);
            staminaCostScale = 1f / PhiMath.PHI4;
        }

        SpendStamina(Mathf.Clamp((1 + HorizontalSpeed) * 0.05f * fatigability, 10, 100) * staminaCostScale);

        moveDirection.y = weakLegs ? jumpPower * 0.5f : jumpPower;
    }

    void HandleMovementState()
    {
        bool crouchHeld = crouchAction.action.IsPressed();

        if (SharedStamina) IsExhausted = Ammo.instance.AtStaminaFloor;
        else if (stamina <= 0f) IsExhausted = true;
        else if (stamina >= staminaRecoveryThreshold) IsExhausted = false;

        bool running = isGroundedStable && !IsExhausted && runAction.action.IsPressed() && anyMovementKeysPressed && !crouchHeld;

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

    void StaminaDeplete() => DrainStaminaAtSprintRate(1f);

    public void DrainStaminaAtSprintRate(float multiplier)
    {
        SpendStamina(Time.deltaTime * fatigability * multiplier);
        if (!SharedStamina) staminaBarDisplay.Refresh(false);
    }

    void StaminaRecovery()
    {

        if (SharedStamina)
        {
            Ammo.instance.StopStaminaSpend();
            return;
        }

        if (isGroundedStable)
        {
            float rate = anyMovementKeysPressed ? 10f : 20f;
            stamina = Mathf.Min(stamina + Time.deltaTime * rate, maxStamina);
        }
        staminaBarDisplay.Refresh(false);
    }

    private bool SharedStamina => PlayerClasses.StaminaSharesMagicka && Ammo.instance;

    public void SpendStamina(float cost)
    {
        if (SharedStamina) Ammo.instance.SpendAsStamina(cost);
        else stamina = Mathf.Max(stamina - cost, 0f);
    }

    public float StaminaFraction => maxStamina > 0f ? Mathf.Clamp01(stamina / maxStamina) : 0f;

    public bool StaminaFull => SharedStamina
        ? Ammo.instance.ammo >= Ammo.instance.maxAmmo
        : stamina >= maxStamina;

    public void AddExplosionJump(float explosionForce, Vector3 explosionCenter, float rangeRadius)
    {
        float forceFactor = Mathf.Clamp01(1 - Vector3.Distance(transform.position, explosionCenter) / rangeRadius);
        Vector3 pushDir = transform.position - explosionCenter;
        pushDir.y = 1f;
        pushDir.Normalize();
        moveDirection += pushDir * (explosionForce * forceFactor);
        if (moveDirection.y > 0f)
        {
            airborneByImpulse = true;
            IsRocketJumping = true;
        }
        footstepSystem.SendJumpSignal();
        camShakeManager.Shake(jumpShake);
    }

    void UpdateGrapplePull()
    {
        Vector3 toTarget = grappleTarget - transform.position;
        float distance = toTarget.magnitude;
        if (distance <= grappleArrivalDistance)
        {
            grappling = false;
            return;
        }
        float scale = grappleSpeed / distance;
        moveDirection = toTarget * scale;
    }

    public void StartGrapple(Vector3 targetPoint, float speed)
    {
        grappling = true;
        grappleTarget = targetPoint;
        grappleSpeed = speed;
        airborneByImpulse = true;
        IsRocketJumping = false;
    }

    public void UpdateGrappleTarget(Vector3 targetPoint)
    {
        if (grappling) grappleTarget = targetPoint;
    }

    public void StopGrapple() => grappling = false;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!grappling) return;
        Vector3 toTarget = grappleTarget - transform.position;
        if (Vector3.Dot(hit.normal, toTarget) < -0.1f * toTarget.magnitude) grappling = false;
    }

    public Vector3 FlatWishDirection(Vector2 input)
    {
        Vector3 direction = forwardPointer.forward * input.y + forwardPointer.right * input.x;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
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

        if (SharedStamina)
        {
            Ammo.instance.GiveManaToPlayer(howMuchStaminaIGive, isSilent);
            return;
        }

        float target = stamina + howMuchStaminaIGive;
        bool maxed = target >= maxStamina;
        stamina = maxed ? maxStamina : target;
        staminaBarDisplay.Refresh(false);
        if (!isSilent) barLightsAnimation.PlaySelectedBarAnimation(1, howMuchStaminaIGive, maxed);
    }

    public void MultiplyMaxSpeed(float factor)
    {
        runSpeed *= factor;
    }

    public void MultiplyJumpPower(float factor)
    {
        jumpPower *= factor;
    }

    public void MultiplyFatigability(float factor)
    {
        fatigability *= factor;
    }

    public void MultiplySpeedDamage(float factor)
    {
        minDamageMultiplier *= factor;
        maxDamageMultiplier *= factor;
    }

    public void MultiplyMaxStamina(float factor)
    {
        float gained = maxStamina * factor - maxStamina;
        maxStamina += gained;
        stamina = Mathf.Min(stamina + gained, maxStamina);
        staminaBarDisplay.Refresh(true);
    }
}
