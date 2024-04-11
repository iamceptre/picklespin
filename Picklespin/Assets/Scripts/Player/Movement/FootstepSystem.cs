using System.Collections;
using UnityEngine;
using FMODUnity;

public class FootstepSystem : MonoBehaviour
{
    public CharacterController controller;
    public PlayerMovement playerMovement;
    public CameraBob cameraBob;

    [HideInInspector] public float horizontalSpeed;
    [HideInInspector] public float verticalSpeed;
    public float overallSpeed;

    public EventReference FootstepEvent;
    public EventReference JumpEvent;


    [SerializeField]private bool isstepping;

    private bool routineRunning = false;

    private float fixedFootstepSpace;

    [Range(0,1)] public float footstepSpaceCooldown;

    private bool isCasting;

    private void Update()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);

        horizontalSpeed = horizontalVelocity.magnitude;
        verticalSpeed = controller.velocity.y;
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
            //any movement key pressed
            FootstepCooldownCalculus();
        }
        else
        {
            //no movement key pressed
            footstepSpaceCooldown = 0f;
        }

        UpdateTimings();

    }

    private void UpdateTimings()
    {
        if (!isCasting) {
            fixedFootstepSpace = 0.8f; //crouch speed

            if (!Input.GetKey(KeyCode.C))
            {
                fixedFootstepSpace = (playerMovement.isRunning ? 0.22f : 0.6f); // run or walk speed
            }

            if (cameraBob.bobSpeed != 0) {
                cameraBob.bobSpeed = 2 / (fixedFootstepSpace);
            }
        }
        else
        {
            fixedFootstepSpace = 0.8f;
            cameraBob.bobSpeed = 2 / (fixedFootstepSpace);
        }
    }

    public void SlowDownDuringCasting()
    {
        isCasting = true;
    }

    public void SpeedMeBackUp()
    {
        isCasting = false;
    }


    void FootstepCooldownCalculus()
    {
        if (footstepSpaceCooldown > 0)
        {
            footstepSpaceCooldown -= Time.deltaTime / fixedFootstepSpace; //and change it to smooth too, when you fix the issue 
        }
        else
        {
            StartCoroutine(SendStepSignalAsync());
            routineRunning = false;
            footstepSpaceCooldown = 1;
        }
    }


    public IEnumerator SendStepSignalAsync()
    {
        if (controller.isGrounded && !routineRunning)
        {
            routineRunning = true;
            RuntimeManager.PlayOneShot(FootstepEvent);
            yield return new WaitForSeconds(Random.Range(0.0f, 0.032f)); //Humanizes footstep rhythm
            yield return new WaitForSeconds(0.05f); //Prevents super fast footsteps when error
            routineRunning = false;
        }
    }

    public IEnumerator SendJumpSignal()
    {
        RuntimeManager.PlayOneShot(JumpEvent);
        yield return null;
    }

}
