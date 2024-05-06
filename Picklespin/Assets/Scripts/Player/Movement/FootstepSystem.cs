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

    private void Update()
    {

        
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            FootstepCooldownCalculus();
        }
        else
        {
            footstepSpaceCooldown = 0f;
        }
       

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
        if (controller.isGrounded)
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
        else
        {
            footstepSpaceCooldown = 0.3f; //resets the countdown when in air, so after landing, you get consistent rhythm
        }
    }


    public IEnumerator SendStepSignalAsync()
    {
        if (!routineRunning)
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
