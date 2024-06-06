using UnityEngine;
using DG.Tweening;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    //LOGIC
    private KeyCode actionKey = KeyCode.E;
    [SerializeField] private bool isLocked = false;
    private bool isOpened;
    private Transform mainCamera;
    private Ray ray;
    private Collider myCollider;
    [SerializeField] LayerMask layerMask;
    private bool buttonBuffer = false;
    private bool canButtonBuffer = true;

    //ROTATION
    private Vector3 startRot;
    private float animationTime = 0.8f;

    //SOUND
    [SerializeField] private EventReference doorOpenSoundEvent;
    [SerializeField] private EventReference doorCloseSoundEvent;
    [SerializeField] private EventReference DoorLockedEvent;
    private EventInstance DoorSoundInstance;

    //TOOLTIP
    public UnityEvent showTooltipEvent;
    public UnityEvent hideTooltipEvent;

    //CACHE
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    void Start()
    {
        startRot = _transform.localEulerAngles;

        DoorSoundInstance = RuntimeManager.CreateInstance(doorOpenSoundEvent);
        DoorSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        myCollider = gameObject.GetComponent<Collider>();
    }


    void Update()
    {
        if (Input.GetKey(actionKey))
        { 
            buttonBuffer = true;
        }
        else
        {
            buttonBuffer = false;
        }

        if (Input.GetKeyUp(actionKey))
        {
            canButtonBuffer = true;
        }


        if (buttonBuffer && canButtonBuffer)
        {
            ray = new Ray(mainCamera.position, mainCamera.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, 5, layerMask))
            {
                if (hit.collider.Equals(myCollider))
                {
                    buttonBuffer = false;
                    LockedCheck();
                    canButtonBuffer = false;
                }
            }
        }
    }


    private void LockedCheck()
    {
        if (!isLocked)
        {
            OpenCloseDecider();
        }
        else
        {
            DoorSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            DoorSoundInstance.release();
            DoorSoundInstance = RuntimeManager.CreateInstance(DoorLockedEvent);
            DoorSoundInstance.start();
        }
    }


    private void OpenCloseDecider()
    {
        if (!isOpened)
        {
            OpenDoor();
            hideTooltipEvent.Invoke();
        }
        else
        {
            CloseDoor();
        }
    }


    private void OpenDoor()
    {
         _transform.DOKill();
         _transform.DOLocalRotate(startRot + new Vector3(0,90), animationTime, RotateMode.Fast);

        DoorSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        DoorSoundInstance.release();
        DoorSoundInstance = RuntimeManager.CreateInstance(doorOpenSoundEvent);
        DoorSoundInstance.start();

        isOpened = true;
    }

    private void CloseDoor()
    {
        _transform.DOKill();
        _transform.DOLocalRotate(startRot, animationTime, RotateMode.Fast);

        DoorSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        DoorSoundInstance.release();
        DoorSoundInstance = RuntimeManager.CreateInstance(doorCloseSoundEvent);

        DoorSoundInstance.start();
        isOpened = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !enabled)
        {
            enabled = true;

            if (!isOpened)
            {
                showTooltipEvent.Invoke();
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && enabled)
        {
            enabled = false;
            hideTooltipEvent.Invoke();
        }
    }

}

