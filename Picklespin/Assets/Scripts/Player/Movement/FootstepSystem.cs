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

        }
    }



    private void PlayFoostepSound()
    {
        if (!isFootstepIgnored) {
            footstepEmitter.Play();

            if (footstepCount % 2 == 0)
            {
                evenFootstepLayerEmitter.Play();
            }
        }
    }


    public IEnumerator IgnoreOneFootstep()
    {
        if (playerMovement.anyMovementKeysPressed)
        {
            isFootstepIgnored = true;
            yield return null;
            isFootstepIgnored = false;
            yield break;
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

    public void SendJumpSignal()
    {

        jumpEventEmitter.Play();
    }

}
