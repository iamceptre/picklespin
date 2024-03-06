using System.Collections;
using UnityEngine;
using FMODUnity;

public class JumpLandSignals : MonoBehaviour
{
    public CameraShake cameraShake;
    public CharacterController characterController;
    public FootstepSystem footstepSystem;
    private bool landed = true;
    private bool routineRunning = false;

    [SerializeField] private EventReference landSoft;
    [SerializeField] private EventReference landHard;


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
            cameraShake.landShakeStrenght = Mathf.Clamp(footstepSystem.overallSpeed * 0.4f, 0, 10); //change it to vertical speed after fixing it not working
        }
    }

    private void Landed()
    {
        if (!routineRunning && !landed)
        {
                StartCoroutine(LandedCooldown());
                //Debug.Log("Falling Velocity is " + cameraShake.landShakeStrenght);
                isLandingHardDecider();
                cameraShake.LandCameraShake();
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
        if (cameraShake.landShakeStrenght >= 5) //is landing hard treshold
        {
            RuntimeManager.PlayOneShot(landHard);
        }
        else
        {
            RuntimeManager.PlayOneShot(landSoft);
        }
    }

}
