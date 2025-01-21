using UnityEngine;
using FMODUnity;
using System.Collections;
using DG.Tweening;

public class JumpLandSignals : MonoBehaviour
{
    public static JumpLandSignals instance;

    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraMovement;

    private FloorTypeDetector floorTypeDetector;
    private CameraBob cameraBob;
    private CameraShakeManagerV2 camShakeManager;
    private PlayerMovement playerMovement;
    private CharacterControllerVelocity speedometer;

    public StudioEventEmitter landSoftEmitter;
    public StudioEventEmitter landHardEmitter;

    private bool landed;
    private bool ignoreFirstLanding = true;
    private bool isFallingLongEnough;
    private float lastLandCameraShakeStrenght;

    [Range(0, 0.5f)]
    [SerializeField] private float fallingTimerCooldown;
    private readonly float fallingTimerTreshold = 0.5f;

    void Awake()
    {
        if (instance && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        floorTypeDetector = FloorTypeDetector.instance;
        speedometer = CharacterControllerVelocity.instance;
        playerMovement = PlayerMovement.Instance;
        camShakeManager = CameraShakeManagerV2.instance;
        cameraBob = CameraBob.instance;
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            Landed();
        }
        else
        {
            if (landed)
            {
                StopAllCoroutines();
                StartCoroutine(FallingTimer());
            }
            landed = false;
            lastLandCameraShakeStrenght = Mathf.Clamp(speedometer.verticalVelocity * 0.4f, 0, 10);
        }
    }

    void Landed()
    {
        if (!landed)
        {
            landed = true;
            ignoreFirstLanding = false;
            playerMovement.externalPushForce = 1;
            floorTypeDetector.Check();

            if (isFallingLongEnough)
            {
                LandCameraMovement(speedometer.verticalVelocity);
                IsLandingHardDecider();
                cameraBob.ResetBobbing();
                StartCoroutine(EnableLandingHearingForAllAI());
            }
        }
    }

    void LandCameraMovement(float strength)
    {
        strength = Mathf.Clamp(strength * 0.1f, 0.1f, 3);
        cameraMovement.DOLocalMoveY(-0.4f * strength, 0.13f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            cameraMovement.DOLocalMoveY(0, 0.2f + (strength * 0.1f)).SetEase(Ease.InOutSine);
        });
        camShakeManager.ShakeHand(strength * 0.1f, 0.2f, 13);
    }

    IEnumerator FallingTimer()
    {
        isFallingLongEnough = false;
        fallingTimerCooldown = 0;

        while (true)
        {
            fallingTimerCooldown += Time.deltaTime;
            yield return null;
            if (fallingTimerCooldown >= fallingTimerTreshold)
            {
                isFallingLongEnough = true;
                yield break;
            }
            if (characterController.isGrounded) yield break;
        }
    }

    void IsLandingHardDecider()
    {
        if (!ignoreFirstLanding)
        {
            if (speedometer.verticalVelocity <= 10)
            {
                camShakeManager.ShakeSelected(0);
                landSoftEmitter.Play();
            }
            else
            {
                camShakeManager.ShakeSelected(1);
                landHardEmitter.Play();
            }
        }
    }

    IEnumerator EnableLandingHearingForAllAI()
    {
        foreach (var ai in AiVision.AllAIs)
        {
            ai.StartCoroutine(ai.EnableLandingHearing());
        }
        yield break;
    }
}
