using System.Collections;
using UnityEngine;
using FMODUnity;

public class JumpLandSignals : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    private FootstepSystem footstepSystem;
    private CameraShake cameraShake;
    private PlayerMovement playerMovement;
    [SerializeField] private HearingRange hearingRange;

    private bool landed = true;
    private bool routineRunning = false;

    [SerializeField] private StudioEventEmitter landSoftEmitter;
    [SerializeField] private StudioEventEmitter landHardEmitter;

    private CharacterControllerVelocity speedometer;


    private bool isFallingLongEnough = true;

    private IEnumerator fallingTimerRoutine;

    private float lastLandCameraShakeStrenght;


    private WaitForSeconds neededFallTimeToSound = new WaitForSeconds(0.5f);
    private WaitForSeconds landCooldownTime = new WaitForSeconds(0.5f);

    private void Start()
    {
        speedometer = CharacterControllerVelocity.instance;
        playerMovement = PlayerMovement.instance;  
        cameraShake = CameraShake.instance;
        footstepSystem = FootstepSystem.instance; 
    }

    private void Update()
    {
        JumpedOrSlipped();
    }

    public void JumpedOrSlipped()
    {
        if (characterController.isGrounded)
        {
            Landed();
        }
        else
        {
            if (landed)
            {
                RoutineTimerManager();
            }

            landed = false;
            lastLandCameraShakeStrenght = Mathf.Clamp(speedometer.verticalVelocity * 0.4f, 0, 10);
        }
    }


    private void RoutineTimerManager()
    {
        isFallingLongEnough = false;
        if (fallingTimerRoutine != null)
        {
            StopCoroutine(fallingTimerRoutine);
            fallingTimerRoutine = null;
        }

        fallingTimerRoutine = FallingTimer();
        StartCoroutine(fallingTimerRoutine);
    }

    private void Landed()
    {
        if (!routineRunning && !landed && speedometer.verticalVelocity > 0.5f)
        {
            StartCoroutine(LandedCooldown());

                isLandingHardDecider();
                footstepSystem.RefreshFootstepTimer();
                footstepSystem.isFootstepIgnored = true;
                cameraShake.LandCameraShake(lastLandCameraShakeStrenght);
                hearingRange.RunExtendedHearingRange();
                playerMovement.externalPushForce = 1;
                landed = true;
        }
    }

    private IEnumerator FallingTimer()        //lets the landing sound play only if falling is greather than ...
    {
        yield return neededFallTimeToSound;
        isFallingLongEnough = true;
    }


    private IEnumerator LandedCooldown()
    {
            routineRunning = true;
        yield return landCooldownTime;
            routineRunning = false;
    }

    private void isLandingHardDecider()
    {
        if (isFallingLongEnough)
        {
            isFallingLongEnough = false;

            if (speedometer.verticalVelocity <= 10) //is landing hard treshold
            {
                landSoftEmitter.Play();
            }
            else
            {
                landHardEmitter.Play();
            }

        }

    }

}
