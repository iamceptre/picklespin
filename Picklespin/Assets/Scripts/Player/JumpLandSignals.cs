using UnityEngine;

public class JumpLandSignals : MonoBehaviour
{

    public CharacterController characterController;
    public FootstepSystem footstepSystem;
    public CameraShake cameraShake;
    private bool inAir = false;
    private bool landed = true;

    void Update()
    {
        JumpUpDetection(); //Remove Update logic, use sending external signals from references instead
    }

    public void JumpUpDetection()
    {
        if (Input.GetKey(KeyCode.Space) && !inAir)
        {
            cameraShake.shakeMultiplier = 0.5f;
            //CameraShake.Invoke();
            inAir = true;
            landed = false;
        }

        if (characterController.isGrounded)
        {
            inAir = false;
            SendLandSignal();
        }
        else
        {
            cameraShake.shakeMultiplier = footstepSystem.overallSpeed * 0.4f;
        }
    }

    private void SendLandSignal()
    {
        if (!landed) {
            CameraShake.Invoke();
            landed = true;
        }
    }
}
