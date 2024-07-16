using System.Collections;
using UnityEngine;
using FMODUnity;

public class FootstepSystem : MonoBehaviour
{
    public static FootstepSystem instance { private set; get; }


    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private StudioEventEmitter footstepEmitter;
    [SerializeField] private StudioEventEmitter evenFootstepLayerEmitter;
    [SerializeField] private StudioEventEmitter jumpEventEmitter;

    private IEnumerator footstepTimerRoutine;


    [SerializeField] private bool isstepping;

    private bool isRoutineRunning = false;

    public float fixedFootstepSpace;
    private float cachedFixedFootstepSpace;
    private WaitForSeconds currentFootstepSpace;

    [HideInInspector] public bool isFootstepIgnored = false;

    private int footstepCount = 0;

    //Footstep Spaces, should be global
    private float walkFootstepSpace = 0.6f;
    private float runFootstepSpace = 0.285f;
    private float sneakFootstepSpace = 0.8f;

    private WaitForSeconds walkFootstepSpaceSecs;
    private WaitForSeconds runFootstepSpaceSecs;
    private WaitForSeconds sneakFootstepSpaceSecs;



    private void Awake()
    {
        walkFootstepSpaceSecs = new WaitForSeconds(walkFootstepSpace);
        runFootstepSpaceSecs = new WaitForSeconds(runFootstepSpace);
        sneakFootstepSpaceSecs = new WaitForSeconds(sneakFootstepSpace);


        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        fixedFootstepSpace = 0.6f; //walk speed
        currentFootstepSpace = new WaitForSeconds(fixedFootstepSpace);
    }

    public void SetNewFootstepSpace(int movementState)
    {
        switch (movementState)
        {
            case 0: //sneak
                fixedFootstepSpace = sneakFootstepSpace;
                currentFootstepSpace = sneakFootstepSpaceSecs;
                break;

            case 1: //walk
                fixedFootstepSpace = walkFootstepSpace;
                currentFootstepSpace = walkFootstepSpaceSecs;
                break;

            case 2: //run
                fixedFootstepSpace = runFootstepSpace;
                currentFootstepSpace = runFootstepSpaceSecs;
                break;

            default: //walk = default
                fixedFootstepSpace = walkFootstepSpace;
                currentFootstepSpace = walkFootstepSpaceSecs;
                break;
        }
    }

    private void Start()
    {

        footstepTimerRoutine = FootstepTimer();
        cachedFixedFootstepSpace = fixedFootstepSpace;
    }

    private void Update()
    {
        if (playerMovement.anyMovementKeysPressed == true && controller.isGrounded) //after making state character controller, replace this if with just call from a given movement state class
        {
            if (!isRoutineRunning)
            {
                isRoutineRunning = true;
                footstepCount = 0;
                StartCoroutine(footstepTimerRoutine);
            }
        }
        else
        {
            StopCoroutine(footstepTimerRoutine);
            isRoutineRunning = false;
        }


        if (fixedFootstepSpace != cachedFixedFootstepSpace)
        {
            RefreshFootstepTimer();
        }

        cachedFixedFootstepSpace = fixedFootstepSpace;

    }

    private IEnumerator FootstepTimer()
    {
        while (isRoutineRunning)
        {
            if (playerMovement.anyMovementKeysPressed == true)
            {
                footstepCount++;
                PlayFoostepSound();
                isFootstepIgnored = false;
                yield return currentFootstepSpace;
            }

        }
    }



    private void PlayFoostepSound()
    {
        if (!isFootstepIgnored)
        {
            footstepEmitter.Play();

            if (footstepCount % 2 == 0)
            {
                evenFootstepLayerEmitter.Play();
            }
        }

        isFootstepIgnored = false;
    }



    public void RefreshFootstepTimer()
    {
        if (playerMovement.anyMovementKeysPressed && controller.isGrounded)
        {
            isRoutineRunning = false;
            StopCoroutine(footstepTimerRoutine);
            isRoutineRunning = true;
            StartCoroutine(footstepTimerRoutine);
        }
    }

    public void SendJumpSignal()
    {
        jumpEventEmitter.Play();
    }

}
