using FMODUnity;
using System.Collections;
using UnityEngine;

public class Dash : MonoBehaviour
{
    private PlayerHP playerHP;
    [SerializeField] private StudioEventEmitter dashEmitter;
    [SerializeField] private int dashStaminaCost = 10;
    [SerializeField] private int dashAmmoCost = 20;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float dashSpeedMultiplier = 1.2f;
    [SerializeField] [Tooltip("Stun Effect Radius")] private float dashEffectRadius = 25f;
    [SerializeField] private AnimationCurve dashDecayCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField] private KeyCode dashKey = KeyCode.Space;

    private CharacterController _characterController;
    private PlayerMovement playerMovement;
    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;
    private StaminaBarDisplay staminaBarDisplay;
    private Ammo ammo;
    private AmmoDisplay ammoDisplay;

    private bool isDashing = false;
    private WaitForSeconds doubleClickThreshold = new WaitForSeconds(0.17f);
    private bool isWaitingForSecondClick = false;

    private TipManager tipManager;
    private const int tipManagerIndex = 7;

    private bool haveEverDashed = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
    private void Start()
    {
        playerMovement = PlayerMovement.instance;
        camShakeManager = CameraShakeManagerV2.instance;
        screenFlashTint = ScreenFlashTint.instance;
        playerHP = PlayerHP.instance;
        tipManager = TipManager.instance;
        staminaBarDisplay = StaminaBarDisplay.instance;
        ammo = Ammo.instance;
        ammoDisplay = AmmoDisplay.instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey) && !isDashing && Input.GetAxisRaw("Horizontal") != 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            if (isWaitingForSecondClick)
            {
                StartCoroutine(PerformDash());
            }
            else
            {
                StartCoroutine(HandleFirstClick());
            }
        }
    }

    private IEnumerator HandleFirstClick()
    {
        isWaitingForSecondClick = true;
        yield return doubleClickThreshold;
        isWaitingForSecondClick = false;
    }

    private IEnumerator PerformDash()
    {
        if (!haveEverDashed)
        {
            haveEverDashed = true;
            tipManager.Hide(tipManagerIndex);
        }

        isDashing = true;
        isWaitingForSecondClick = false;
        playerHP.invincible = true;
        dashEmitter.Play();
        camShakeManager.ShakeSelected(11);
        screenFlashTint.Flash(5);
        TakeStats();

        Vector3 dashDirection = playerMovement.moveDirection.normalized;
        dashDirection.y = 0;

        AffectNearbyAIPaths();

        float originalSpeedMultiplier = playerMovement.speedMultiplier;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            float decayFactor = dashDecayCurve.Evaluate(elapsedTime / dashDuration);
            playerMovement.speedMultiplier = Mathf.Lerp(originalSpeedMultiplier, dashSpeedMultiplier, decayFactor);

            playerMovement.characterController.Move(dashDirection * playerMovement.runSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerMovement.speedMultiplier = originalSpeedMultiplier;
        //playerMovement.GiveStaminaToPlayer(-dashStaminaCost);
        playerHP.invincible = false;
        StartCoroutine(WaitForGrounded());
    }

    private void AffectNearbyAIPaths()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, dashEffectRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            StopAiForAsec aiStopper = hitCollider.GetComponent<StopAiForAsec>();
            if (aiStopper != null)
            {
                aiStopper.StopMeForASec();
            }
        }
    }


    private IEnumerator WaitForGrounded()
    {
        while (!_characterController.isGrounded)
        {
            yield return null;
        }
        isDashing = false;
    }


    public void ShowDashTip()
    {
        tipManager.ShowAndHide(tipManagerIndex);
    }


    private void TakeStats()
    {

        if (playerMovement.stamina > dashStaminaCost)
        {
            playerMovement.stamina -= dashStaminaCost;
        }
        else
        {
            playerMovement.stamina = 0;
        }


        if (ammo.ammo > dashAmmoCost)
        {
            ammo.ammo -= dashAmmoCost;
        }
        else
        {
            ammo.ammo = 0;
        }

        ammoDisplay.Refresh(false);
        staminaBarDisplay.Refresh(false);
    }
}