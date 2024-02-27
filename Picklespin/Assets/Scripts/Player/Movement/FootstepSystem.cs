using System.Collections;
using UnityEngine;
using FMODUnity;

public class FootstepSystem : MonoBehaviour
{
    public CharacterController controller;
    public PlayerMovement playerMovement;
    public CameraBob cameraBob;

    public float horizontalSpeed;
    public float verticalSpeed;
    public float overallSpeed;
    public EventReference FootstepEvent;
    public EventReference JumpEvent;


    [SerializeField]private bool isstepping;

   private bool movementKeyPressed = false;
   private bool routineRunning = false;

  //  [SerializeField]public float footstepTimeSpace;


  // [SerializeField] private float baseTimeSpace;
  // [SerializeField] private float horizontalMul;

    public float fixedFootstepSpace;

   // private bool isJumping;


    private void Update()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

        // The speed on the x-z plane ignoring any speed
        horizontalSpeed = horizontalVelocity.magnitude;
    // The speed from gravity or jumping
    verticalSpeed = controller.velocity.y;
        // The overall speed
   overallSpeed = controller.velocity.magnitude;

        if (!isstepping && controller.isGrounded && horizontalSpeed >= 1f)
        {
            isstepping = true;
        }

       if (isstepping && horizontalSpeed <= 1f || !controller.isGrounded)
      {
           isstepping = false;
      }


        if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S)) || (Input.GetKey(KeyCode.A)) || (Input.GetKey(KeyCode.D)))
        {
            movementKeyPressed = true;
        }
        else
        {
            movementKeyPressed = false;
        }


        if (movementKeyPressed && !routineRunning)
        {
            StartCoroutine(SendStepSignal());
        }

        if (!isstepping)
        {
            StopCoroutine(SendStepSignal());
        }

      // footstepTimeSpace = Mathf.Abs(1f - Mathf.Clamp((horizontalSpeed * 0.1f),0, 0.76f)); // footstep impulse generator based on speed


    }

    IEnumerator SendStepSignal() //fix the delayed step when changing speed, add async Forced Update when speed is changed
    {
        if (movementKeyPressed && controller.isGrounded && !routineRunning && isstepping)
        {
            routineRunning = true;
            RuntimeManager.PlayOneShot(FootstepEvent);
            fixedFootstepSpace = 0.8f; //crouch speed
            if (!Input.GetKey(KeyCode.C))
            {
                fixedFootstepSpace = (playerMovement.isRunning ? 0.22f : 0.6f); // run or walk speed
            }

            cameraBob.bobSpeed = (1 / (fixedFootstepSpace + 0.0001f)) * 2;

            yield return new WaitForSeconds(Random.Range(0.0f, 0.032f)); //Humanizes footstep rhythm

            yield return new WaitForSeconds(fixedFootstepSpace);

            routineRunning = false;
            StartCoroutine(SendStepSignal());
        }
    }

    public IEnumerator SendJumpSignal()
    {
        RuntimeManager.PlayOneShot(JumpEvent);
        yield return null;
    }

}
