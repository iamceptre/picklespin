using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FootstepSystem : MonoBehaviour
{
    public static FootstepSystem instance { private set; get; }


    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference evenFootstepLayerEvent;  
    public EventInstance footstepInstance;
    [SerializeField] private EventReference jumpEvent;

    private IEnumerator footstepTimerRoutine;


    [SerializeField] private bool isstepping;

    private bool isRoutineRunning = false;

    public float fixedFootstepSpace;
    private float cachedFixedFootstepSpace;

    [HideInInspector] public bool isFootstepIgnored = false;

    private int footstepCount = 0;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        fixedFootstepSpace = 0.6f; //walk speed
        footstepTimerRoutine = FootstepTimer();
        cachedFixedFootstepSpace = fixedFootstepSpace;
        footstepInstance = RuntimeManager.CreateInstance(footstepEvent);
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
        while (isRoutineRunning) {
            if (playerMovement.anyMovementKeysPressed == true)
            {
                float randomHumanize = Random.Range(0, 0.032f);
                footstepCount++;
                PlayFoostepSound();
                yield return new WaitForSeconds(fixedFootstepSpace + randomHumanize);
            }
            else
            {
                footstepInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }



    private void PlayFoostepSound()
    {
        if (!isFootstepIgnored)
        {
            footstepInstance.start();
        }

        if (footstepCount % 2 == 0)
        {
            RuntimeManager.PlayOneShot(evenFootstepLayerEvent);
        }

    }


    public IEnumerator IgnoreOneFootstep()
    {
        if (playerMovement.anyMovementKeysPressed)
        {
            isFootstepIgnored = true;
            yield return null;
            isFootstepIgnored = false;
        }
    }



    public void RefreshFootstepTimer()
    {
        if (playerMovement.anyMovementKeysPressed && controller.isGrounded) {
            isRoutineRunning = false;
            StopCoroutine(footstepTimerRoutine);
            isRoutineRunning = true;
            StartCoroutine(footstepTimerRoutine);
        }
    }

    public IEnumerator SendJumpSignal() //change it to void tf
    {
        RuntimeManager.PlayOneShot(jumpEvent);
        yield return null;
    }

}
