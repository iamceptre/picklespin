using UnityEngine;
using DG.Tweening;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    //LOGIC
    [SerializeField] private bool isLocked = false;
    private bool isOpened;
    private Transform mainCamera;
    private Ray ray;
    private Collider myCollider;
    [SerializeField] LayerMask layerMask;

    //ROTATION
    private Vector3 startRot;
    private Vector3 openedRot;

    //SOUND
    [SerializeField] private EventReference doorOpenSoundEvent;
    [SerializeField] private EventReference doorCloseSoundEvent;
    [SerializeField] private EventReference DoorLockedEvent;
    private EventInstance DoorSoundInstance;

    //TOOLTIP
    public UnityEvent showTooltipEvent;
    public UnityEvent hideTooltipEvent;

    void Start()
    {
        startRot = transform.eulerAngles;
        openedRot = startRot + new Vector3(0, 0, 90);

        DoorSoundInstance = RuntimeManager.CreateInstance(doorOpenSoundEvent);
        DoorSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        myCollider = gameObject.GetComponent<Collider>();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ray = new Ray(mainCamera.position, mainCamera.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, 5, layerMask))
            {
                if (hit.collider.Equals(myCollider))
                {
                    LockedCheck();
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
        transform.DOKill();
        transform.DOLocalRotate(openedRot, 1, RotateMode.Fast);

        DoorSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        DoorSoundInstance.release();
        DoorSoundInstance = RuntimeManager.CreateInstance(doorOpenSoundEvent);
        DoorSoundInstance.start();

        isOpened = true;
    }

    private void CloseDoor()
    {
        transform.DOKill();
        transform.DOLocalRotate(startRot, 1, RotateMode.Fast);

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

