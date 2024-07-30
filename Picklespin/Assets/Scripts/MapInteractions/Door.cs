using UnityEngine;
using DG.Tweening;
using FMODUnity;
using FMOD.Studio;

public class Door : MonoBehaviour
{
    //LOGIC
    private KeyCode actionKey = KeyCode.E;
    public bool isLocked = false;
    private bool isOpened;
    private Transform mainCamera;
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

    //TOOLTIPS
    private TipManager tipManager;

    //CACHE
    [SerializeField]private Transform _transform;
    private Vector3 rotationVector = new Vector3(0, 0, 90);

    //EXTERNAL REFERENCES 
    private Animator handAnimator;

    private void Awake()
    {
        if(_transform == null)
        _transform = transform;
    }

    private void OnEnable()
    {
        tipManager = TipManager.instance;
    }

    void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
        startRot = _transform.localEulerAngles;

        DoorSoundInstance = RuntimeManager.CreateInstance(doorOpenSoundEvent);
        DoorSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        mainCamera = CachedCameraMain.instance.transform;
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
                    buttonBuffer = false;
                    LockedCheck();
                    canButtonBuffer = false;
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
            handAnimator.SetTrigger("Hand_Fail");
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
            handAnimator.SetTrigger("Door_Open");
            OpenDoor();
           // tipManager.Hide(0);
        }
        else
        {
            handAnimator.SetTrigger("Door_Close");
            CloseDoor();
        }
    }


    private void OpenDoor()
    {
         _transform.DOKill();
         _transform.DOLocalRotate(startRot + rotationVector, animationTime, RotateMode.Fast);

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
            if (!isLocked)
            {
                tipManager.Show(0);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && enabled)
        {
            enabled = false;
            tipManager.Hide(0);
        }
    }

}

