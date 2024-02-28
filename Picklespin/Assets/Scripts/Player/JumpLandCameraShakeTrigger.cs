using UnityEngine;

public class JumpLandCameraShakeTrigger : MonoBehaviour
{

    public CharacterController characterController;
    public FootstepSystem footstepSystem;
    public CameraShake cameraShake;
    private bool inAir = false;
    private bool landed = true;

    void Update()
    {
        JumpUpDetection();
    }

    public void JumpUpDetection()
    {
        if (Input.GetKey(KeyCode.Space) && !inAir)
        {
            cameraShake.shakeMultiplier = 0.5f;
            CameraShake.Invoke();
            inAir = true;
            landed = false;
        }

        if (characterController.isGrounded)
        {
            inAir = false;
            LandDetection();
        }
        else
        {
            cameraShake.shakeMultiplier = Mathf.Round(Mathf.Abs(footstepSystem.overallSpeed))*0.5f;
        }
    }

    private void LandDetection()
    {
        if (!landed) {
            CameraShake.Invoke();
            landed = true;
        }
    }
}
