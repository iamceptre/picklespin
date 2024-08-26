using FMODUnity;
using System.Collections;
using UnityEngine;

public class Dash : MonoBehaviour
{
    private PlayerHP playerHP;
    [SerializeField] private StudioEventEmitter dashEmitter;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float dashSpeedMultiplier = 1.2f;
    [SerializeField] private float dashEffectRadius = 25f;
    [SerializeField] private AnimationCurve dashDecayCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    [SerializeField] private KeyCode dashKey = KeyCode.Space;

    private CharacterController _characterController;
    private PlayerMovement playerMovement;
    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;
    private bool isDashing = false;
    private WaitForSeconds doubleClickThreshold = new WaitForSeconds(0.17f);
    private bool isWaitingForSecondClick = false;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey) && !isDashing && Input.GetAxisRaw("Horizontal") != 0)
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
        isDashing = true;
        isWaitingForSecondClick = false;
        playerHP.invincible = true;
        dashEmitter.Play();
        camShakeManager.ShakeSelected(11);
        screenFlashTint.Flash(5);

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
}