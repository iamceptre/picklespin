using UnityEngine;
using FMODUnity;
using System.Collections;
using DG.Tweening;

public class JumpLandSignals : MonoBehaviour
{
    public static JumpLandSignals instance;

    private FloorTypeDetector floorTypeDetector;

    [SerializeField] private CharacterController characterController;
    private CameraBob cameraBob;
    private CameraShakeManagerV2 camShakeManager;
    [SerializeField] private Transform cameraMovement;
    private PlayerMovement playerMovement;
    [SerializeField] private HearingRange hearingRange;

    [SerializeField] private bool landed = false;

    public StudioEventEmitter landSoftEmitter;
    public StudioEventEmitter landHardEmitter;

    private CharacterControllerVelocity speedometer;

    private float lastLandCameraShakeStrenght;

    private bool ignoreFirstLanding = true;

    [Range(0, 0.5f)][SerializeField] private float fallingTimerCooldown;
    private float fallingTimerTreshold = 0.5f;
    public bool isFallingLongEnough = false;

    private void Awake()
    {
        if (instance != null && instance != this)
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
        floorTypeDetector = FloorTypeDetector.instance;
        speedometer = CharacterControllerVelocity.instance;
        playerMovement = PlayerMovement.Instance;
        camShakeManager = CameraShakeManagerV2.instance;
        cameraBob = CameraBob.instance;
    }

    private void Update()
    {
        if (characterController.isGrounded)
        {
            Landed(); //one tick
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



    private void Landed()
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
                isLandingHardDecider();
                cameraBob.ResetBobbing();
                //footstepSystem.RefreshFootstepTimer();
                hearingRange.RunExtendedHearingRange();
            }
        }
    }


    private void LandCameraMovement(float strenght)
    {
        strenght = Mathf.Clamp(strenght * 0.1f, 0.1f, 3);
        //Debug.Log("land bounce strenght: " + strenght);
        cameraMovement.DOLocalMoveY(-0.4f * strenght, 0.13f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            cameraMovement.DOLocalMoveY(0, 0.2f + (strenght*0.1f)).SetEase(Ease.InOutSine);
        });

        camShakeManager.ShakeHand(strenght * 0.1f, 0.2f, 13);

    }


    private IEnumerator FallingTimer()
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

            if (characterController.isGrounded)
            {
                yield break;
            }

        }


    }




    private void isLandingHardDecider()
    {
        if (!ignoreFirstLanding)
        {

            if (speedometer.verticalVelocity <= 10) //is landing hard treshold
            {
                camShakeManager.ShakeSelected(0); //add organic shake multiplier based on jump velocity
                landSoftEmitter.Play();
            }
            else
            {
                camShakeManager.ShakeSelected(1);
                landHardEmitter.Play();
            }

        }
    }
}
