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
    private CharacterControllerVelocity speedometer;

    public StudioEventEmitter landSoftEmitter;
    public StudioEventEmitter landHardEmitter;

    [Header("Camera shake")]
    [SerializeField] private CameraShakeSettings landSoftShake = new()
    {
        rotationAmount = new Vector3(0.27f, 0.07f, 0.07f), numberOfShakes = 2, speed = 35f, decay = 0.7f, uiShakeModifier = 1f
    };
    [SerializeField] private CameraShakeSettings landHardShake = new()
    {
        rotationAmount = new Vector3(0.4f, 0.1f, 0.3f), numberOfShakes = 5, speed = 60f, decay = 0.6f, uiShakeModifier = 1f
    };

    private bool landed;
    private bool ignoreFirstLanding = true;
    private bool isFallingLongEnough;

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
        }
    }

    void Landed()
    {
        if (!landed)
        {
            landed = true;
            ignoreFirstLanding = false;
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
                camShakeManager.Shake(landSoftShake);
                landSoftEmitter.Play();
            }
            else
            {
                camShakeManager.Shake(landHardShake);
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
