using System.Collections;
using UnityEngine;
using FMODUnity;

public class FootstepSystem : MonoBehaviour
{
    public static FootstepSystem instance { private set; get; }
    private FloorTypeDetector floorTypeDetector;


    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement playerMovement;
    public StudioEventEmitter footstepEmitter;
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
        //THOOSE ARE IN HALF
        walkFootstepSpaceSecs = new WaitForSeconds(walkFootstepSpace * 0.5f);
        runFootstepSpaceSecs = new WaitForSeconds(runFootstepSpace * 0.5f);
        sneakFootstepSpaceSecs = new WaitForSeconds(sneakFootstepSpace * 0.5f);


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

    private void Start()
    {
        floorTypeDetector = FloorTypeDetector.instance;
        footstepTimerRoutine = FootstepTimer();
        cachedFixedFootstepSpace = fixedFootstepSpace;
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

    private void Update()
    {
        if (playerMovement.anyMovementKeysPressed)
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
            yield return currentFootstepSpace;
            footstepCount++;
            floorTypeDetector.Check();
            PlayFoostepSound();
            isFootstepIgnored = false;
            yield return currentFootstepSpace;
        }
    }



    private void PlayFoostepSound()
    {
        if (!isFootstepIgnored && controller.isGrounded)
        {
            footstepEmitter.Play();

            if (footstepCount % 2 == 0)
            {
                evenFootstepLayerEmitter.Play();
            }
            isFootstepIgnored = false;
        }
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
