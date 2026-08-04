using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : MonoBehaviour
{
    public static Dash Instance { get; private set; }

    [SerializeField] StudioEventEmitter dashEmitter;
    [SerializeField] int dashStaminaCost = 10;
    [SerializeField] int dashAmmoCost = 20;
    [SerializeField] float dashDuration = 0.4f;
    [SerializeField] float dashSpeedMultiplier = 1.2f;
    [SerializeField] float dashEffectRadius = 25f;
    [SerializeField, Tooltip("what the dash shockwave stuns - falls back to the EvilEntity layer when left empty")]
    LayerMask stunLayers;
    [SerializeField, Tooltip("shared with the spell cooldown bar: a dash blocks casting for this long, and a spell's cooldown blocks the dash")]
    float dashCooldown = 2f;
    [SerializeField] AnimationCurve dashDecayCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField] CameraShakeSettings dashShake = new()
    {
        rotationAmount = new Vector3(0.4f, 0.4f, 0.4f), numberOfShakes = 6, speed = 50f, decay = 0.5f, uiShakeModifier = 0f
    };
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference moveAction;

    static readonly Collider[] overlapResults = new Collider[64];
    static readonly HashSet<AiReferences> dashHitBuffer = new();

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;
    private StaminaBarDisplay staminaBarDisplay;
    private Ammo ammo;
    private AmmoDisplay ammoDisplay;
    private PlayerHP playerHP;
    private TipManager tipManager;
    private Attack attack;

    bool isDashing;
    bool isWaitingForSecondClick;
    bool haveEverDashed;
    private readonly WaitForSeconds doubleClickThreshold = new(0.17f);

    private const float MoveDeadzone = 0.01f;

    private const string EnemyLayer = "EvilEntity";

    void Awake()
    {
        if (Instance && Instance != this) Destroy(this);
        else Instance = this;

        if (stunLayers == 0) stunLayers = LayerMask.GetMask(EnemyLayer);

        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        playerMovement = PlayerMovement.Instance;
        camShakeManager = CameraShakeManagerV2.instance;
        screenFlashTint = ScreenFlashTint.instance;
        staminaBarDisplay = StaminaBarDisplay.instance;
        ammo = Ammo.instance;
        ammoDisplay = AmmoDisplay.instance;
        playerHP = PlayerHP.Instance;
        tipManager = TipManager.instance;
        attack = Attack.instance;
    }

    void Update()
    {

        if (isDashing || (attack && !attack.CooldownReady)) return;
        Vector2 moveValue = moveAction.action.ReadValue<Vector2>();
        if (dashAction.action.triggered && moveValue.sqrMagnitude > MoveDeadzone)
        {
            if (isWaitingForSecondClick) StartCoroutine(DashRoutine());
            else StartCoroutine(FirstClick());
        }
    }

    IEnumerator FirstClick()
    {
        isWaitingForSecondClick = true;
        yield return doubleClickThreshold;
        isWaitingForSecondClick = false;
    }

    IEnumerator DashRoutine()
    {
        if (!haveEverDashed)
        {
            haveEverDashed = true;
            if (tipManager) tipManager.Hide(7);
        }
        isDashing = true;
        isWaitingForSecondClick = false;
        playerHP.invincible = true;
        dashEmitter.Play();
        camShakeManager.Shake(dashShake);
        screenFlashTint.Flash(5);
        ConsumeStats();
        if (attack) attack.BeginCooldown(dashCooldown);

        Vector3 dashDirection = playerMovement.moveDirection;
        dashDirection.y = 0;
        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = playerMovement.FlatWishDirection(moveAction.action.ReadValue<Vector2>());
        dashDirection.Normalize();
        int hitsCount = Physics.OverlapSphereNonAlloc(transform.position, dashEffectRadius, overlapResults, stunLayers);
        int cutDamage = LightfootUpgrades.DashDamage;
        dashHitBuffer.Clear();
        for (int i = 0; i < hitsCount; i++)
        {

            StopAiForAsec stopper = overlapResults[i].GetComponentInParent<StopAiForAsec>();
            if (stopper) stopper.StopMeForASec();

            if (cutDamage <= 0) continue;

            AiReferences refs = overlapResults[i].GetComponentInParent<AiReferences>();
            if (!refs || refs.IsAngel || !dashHitBuffer.Add(refs)) continue;
            if (ConvertedAlly.IsConverted(refs)) continue;
            if (refs.Health && refs.Health.IsAlive) refs.Health.TakeQuietDamage(cutDamage);
        }
        float originalSpeed = playerMovement.speedMultiplier;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float factor = dashDecayCurve.Evaluate(elapsed / dashDuration);
            playerMovement.speedMultiplier = Mathf.Lerp(originalSpeed, dashSpeedMultiplier, factor);
            playerMovement.characterController.Move(playerMovement.runSpeed * Time.deltaTime * dashDirection);

            playerMovement.ReportExternalVelocity(playerMovement.runSpeed * dashDirection);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerMovement.speedMultiplier = originalSpeed;
        playerHP.invincible = false;
        StartCoroutine(WaitGrounded());
    }

    IEnumerator WaitGrounded()
    {
        while (!characterController.isGrounded) yield return null;
        isDashing = false;
    }

    void ConsumeStats()
    {
        playerMovement.SpendStamina(dashStaminaCost);
        if (ammo.ammo > dashAmmoCost) ammo.ammo -= dashAmmoCost;
        else ammo.ammo = 0;
        ammoDisplay.Refresh(false);
        staminaBarDisplay.Refresh(false);
        ammo.MagickaChanged();
    }

    public void MultiplyDashPower(float factor)
    {
        dashDuration *= factor;
        dashSpeedMultiplier *= factor;
    }

    public void MultiplyDashRadius(float factor)
    {
        dashEffectRadius *= factor;
    }
}
