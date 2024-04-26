using System.Collections;
using UnityEngine;
using FMODUnity;

public class FootstepSystem : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraBob cameraBob;

    [SerializeField] private EventReference FootstepEvent;
    [SerializeField] private EventReference JumpEvent;


    [SerializeField] private bool isstepping;

    private bool routineRunning = false;

    private float fixedFootstepSpace;

    [Range(0, 1)] public float footstepSpaceCooldown;

    private bool isCasting;

    //private CharacterControllerVelocity speedometer;


    private void Start()
    {
       // speedometer = CharacterControllerVelocity.instance;
    }

    private void Update()
    {

       
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            //any movement key pressed
            FootstepCooldownCalculus();
        }
        else
        {
            //no movement key pressed
            footstepSpaceCooldown = 0f;
        }
        

        /*
        //better compatibility but not as responsive
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            FootstepCooldownCalculus();
        }
        else
        {
            footstepSpaceCooldown = 0f;
        }
        */

        UpdateTimings();

    }

    private void UpdateTimings()
    {
        if (!isCasting)
        {
            fixedFootstepSpace = 0.8f; //crouch speed

            if (!Input.GetKey(KeyCode.C))
            {
                fixedFootstepSpace = (playerMovement.isRunning ? 0.22f : 0.6f); // run or walk speed
            }

            if (cameraBob.bobSpeed != 0)
            {
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
            routineRunning = false;
        }
    }

    public IEnumerator SendJumpSignal()
    {
        RuntimeManager.PlayOneShot(JumpEvent);
        yield return null;
    }

}
