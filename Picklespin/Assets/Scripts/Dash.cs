using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : MonoBehaviour
{
    [SerializeField] StudioEventEmitter dashEmitter;
    [SerializeField] int dashStaminaCost = 10;
    [SerializeField] int dashAmmoCost = 20;
    [SerializeField] float dashDuration = 0.4f;
    [SerializeField] float dashSpeedMultiplier = 1.2f;
    [SerializeField] float dashEffectRadius = 25f;
    [SerializeField] AnimationCurve dashDecayCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference moveAction;

    static readonly Collider[] overlapResults = new Collider[10];

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;
    private StaminaBarDisplay staminaBarDisplay;
    private Ammo ammo;
    private AmmoDisplay ammoDisplay;
    private PlayerHP playerHP;
    private TipManager tipManager;

    bool isDashing;
    bool isWaitingForSecondClick;
    bool haveEverDashed;
    private readonly WaitForSeconds doubleClickThreshold = new(0.17f);

    void Awake()
    {
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
    }

    void Update()
    {
        if (isDashing) return;
        Vector2 moveValue = moveAction.action.ReadValue<Vector2>();
        if (dashAction.action.triggered && moveValue.x != 0 && Mathf.Abs(moveValue.y) < 0.001f)
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
            tipManager.Hide(7);
        }
        isDashing = true;
        isWaitingForSecondClick = false;
        playerHP.invincible = true;
        dashEmitter.Play();
        camShakeManager.ShakeSelected(11);
        screenFlashTint.Flash(5);
        ConsumeStats();
        Vector3 dashDirection = playerMovement.moveDirection.normalized;
        dashDirection.y = 0;
        int hitsCount = Physics.OverlapSphereNonAlloc(transform.position, dashEffectRadius, overlapResults);
        for (int i = 0; i < hitsCount; i++)
        {
            StopAiForAsec stopper = overlapResults[i].GetComponent<StopAiForAsec>();
            if (stopper) stopper.StopMeForASec();
        }
        float originalSpeed = playerMovement.speedMultiplier;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float factor = dashDecayCurve.Evaluate(elapsed / dashDuration);
            playerMovement.speedMultiplier = Mathf.Lerp(originalSpeed, dashSpeedMultiplier, factor);
            playerMovement.characterController.Move(playerMovement.runSpeed * Time.deltaTime * dashDirection);
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
        if (playerMovement.stamina > dashStaminaCost) playerMovement.stamina -= dashStaminaCost;
        else playerMovement.stamina = 0;
        if (ammo.ammo > dashAmmoCost) ammo.ammo -= dashAmmoCost;
        else ammo.ammo = 0;
        ammoDisplay.Refresh(false);
        staminaBarDisplay.Refresh(false);
    }

    public void ShowDashTip()
    {
        tipManager.ShowAndHide(7);
    }
}
