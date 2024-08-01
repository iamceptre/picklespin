using UnityEngine;
using DG.Tweening;
using FMODUnity;

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
    [SerializeField] private StudioEventEmitter doorOpenSound;
    [SerializeField] private StudioEventEmitter doorCloseSound;
    [SerializeField] private StudioEventEmitter doorLockedSound;

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
            doorLockedSound.Play();
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

        doorLockedSound.Stop();
        doorOpenSound.Play();

        isOpened = true;
    }

    private void CloseDoor()
    {
        _transform.DOKill();
        _transform.DOLocalRotate(startRot, animationTime, RotateMode.Fast);

        doorOpenSound.Stop();
        doorCloseSound.Play();

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

