using UnityEngine;
using DG.Tweening;
using FMODUnity;
using UnityEngine.InputSystem;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Configurable Parameters")]
    [SerializeField] private LayerMask layerMask;  // Assign this to only the "Door" layer
    [SerializeField] private Transform _transform;
    [SerializeField] private StudioEventEmitter doorOpenSound;
    [SerializeField] private StudioEventEmitter doorCloseSound;
    [SerializeField] private StudioEventEmitter doorLockedSound;

    private static readonly WaitForSeconds refreshRate = new WaitForSeconds(0.5f);
    private static readonly Vector3 rotationVector = new Vector3(0, 0, 90);
    private static readonly float animationTime = 0.8f;
    private static readonly float maxDistance = 7f;

    [Header("Logic")]
    public bool isLocked = false;
    private bool isOpened = false;
    private bool canButtonBuffer = true;

    [Header("Cache")]
    private Transform mainCamera;
    private Animator handAnimator;
    private TipManager tipManager;
    [SerializeField] private Collider myCollider;
    private Vector3 startRot;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference interactAction;

    private void Awake()
    {
        _transform = _transform != null ? _transform : transform;
        startRot = _transform.localEulerAngles;
    }

    private void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
        mainCamera = CachedCameraMain.instance.cachedTransform;
        tipManager = TipManager.instance;
        enabled = false;
    }

    private void OnEnable()
    {
        interactAction.action.started += OnInteractStarted;
        interactAction.action.canceled += OnInteractCanceled;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.started -= OnInteractStarted;
        interactAction.action.canceled -= OnInteractCanceled;
        interactAction.action.Disable();
    }

    private void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        if (canButtonBuffer)
        {
            canButtonBuffer = false;
            PerformRaycastCheck();
        }
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        canButtonBuffer = true;
    }

    private void PerformRaycastCheck()
    {
        if (Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, 5f, layerMask))
        {
            if (hit.collider == myCollider)
            {
                if (isLocked)
                {
                    handAnimator.SetTrigger("Hand_Fail");
                    doorLockedSound.Play();
                }
                else
                {
                    ToggleDoor();
                }
            }
        }
    }

    private void ToggleDoor()
    {
        tipManager.Hide(0);
        if (isOpened) CloseDoor(); else OpenDoor();
    }

    private void OpenDoor()
    {
        _transform.DOKill();
        _transform.DOLocalRotate(startRot + rotationVector, animationTime, RotateMode.Fast);
        doorCloseSound.Stop();
        doorOpenSound.Play();
        isOpened = true;
        handAnimator.SetTrigger("Door_Open");
    }

    private void CloseDoor()
    {
        _transform.DOKill();
        _transform.DOLocalRotate(startRot, animationTime, RotateMode.Fast);
        doorOpenSound.Stop();
        doorCloseSound.Play();
        isOpened = false;
        handAnimator.SetTrigger("Door_Close");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enabled = true;
            StopAllCoroutines();
            StartCoroutine(DisableWhenPlayerGoesAway());
            if (!isLocked) tipManager.Show(0);
        }
    }

    private IEnumerator DisableWhenPlayerGoesAway()
    {
        while (enabled)
        {
            yield return refreshRate;
            if (Vector3.Distance(mainCamera.position, _transform.position) > maxDistance)
            {
                tipManager.Hide(0);
                enabled = false;
            }
        }
    }
}
