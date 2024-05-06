using System.Collections;
using UnityEngine;
using FMODUnity;

public class JumpLandSignals : MonoBehaviour
{
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FootstepSystem footstepSystem;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private HearingRange hearingRange;

    private bool landed = true;
    private bool routineRunning = false;

    [SerializeField] private EventReference landSoft;
    [SerializeField] private EventReference landHard;

    private CharacterControllerVelocity speedometer;

    private bool skipFirstSound;


    private void Start()
    {
        skipFirstSound = true;
        speedometer = CharacterControllerVelocity.instance;
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
            landed = false;
            cameraShake.landShakeStrenght = Mathf.Clamp(speedometer.verticalVelocity * 0.4f, 0, 10);
        }
    }

    private void Landed()
    {

        if (!routineRunning && !landed && speedometer.verticalVelocity > 0.5f)
        {
                StartCoroutine(LandedCooldown());
            //Debug.Log("Falling Velocity is " + cameraShake.landShakeStrenght);
            if (!skipFirstSound)
            {
                isLandingHardDecider();
            }
                skipFirstSound = false;
                cameraShake.LandCameraShake();
                hearingRange.RunExtendedHearingRange();
                playerMovement.externalPushForce = 1;
                landed = true;
        }
    }


    private IEnumerator LandedCooldown()
    {
            routineRunning = true;
            yield return new WaitForSeconds(0.5f);
            routineRunning = false;
    }

    private void isLandingHardDecider()
    {
        if (speedometer.verticalVelocity >= 10) //is landing hard treshold
        {
            RuntimeManager.PlayOneShot(landHard);
        }
        else
        {
            RuntimeManager.PlayOneShot(landSoft);
        }
    }

}
