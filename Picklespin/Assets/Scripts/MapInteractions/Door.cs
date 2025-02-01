using UnityEngine;
using DG.Tweening;
using FMODUnity;
using UnityEngine.InputSystem;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform _transform;
    [SerializeField] private StudioEventEmitter doorOpenSound;
    [SerializeField] private StudioEventEmitter doorCloseSound;
    [SerializeField] private StudioEventEmitter doorLockedSound;

    private static readonly WaitForSeconds refreshRate = new(0.04f);
    private static readonly Vector3 rotationVector = new(0, 0, 90);
    private static readonly float animationTime = 0.8f;
    private static readonly float maxDistance = 7f;

    [Header("Logic")]
    public bool isLocked;
    private bool isOpened;
    private bool canButtonBuffer = true;
    private bool playerInRange;

    [Header("References")]
    [SerializeField] private Collider myCollider;
    [SerializeField] private InputActionReference interactAction;

    private Transform mainCamera;
    private Animator handAnimator;
    private TipManager tipManager;
    private CrosshairManager crosshair;
    private Vector3 startRot;

    private void Awake()
    {
        if (!_transform) _transform = transform;
        startRot = _transform.localEulerAngles;
    }

    private void Start()
    {
        crosshair = CrosshairManager.Instance;
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
        if (!canButtonBuffer) return;
        canButtonBuffer = false;
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
                    tipManager.Hide(0);
                    if (isOpened) CloseDoor(); else OpenDoor();
                }
            }
        }
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        canButtonBuffer = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            enabled = true;
            if (!isLocked) tipManager.Show(0);
            StopAllCoroutines();
            StartCoroutine(CheckDoorRangeAndSight());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            tipManager.Hide(0);
            crosshair.HideCrosshair();
            enabled = false;
        }
    }

    private IEnumerator CheckDoorRangeAndSight()
    {
        bool wasLookingAtDoor = false;
        while (playerInRange)
        {
            yield return refreshRate;
            if (Vector3.Distance(mainCamera.position, _transform.position) > maxDistance)
            {
                playerInRange = false;
                tipManager.Hide(0);
                crosshair.HideCrosshair();
                enabled = false;
                yield break;
            }
            bool isLookingAtDoor =
                Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, 5f, layerMask)
                && hit.collider == myCollider;
            if (isLookingAtDoor && !wasLookingAtDoor) crosshair.ShowCrosshair();
            else if (!isLookingAtDoor && wasLookingAtDoor) crosshair.HideCrosshair();
            wasLookingAtDoor = isLookingAtDoor;
        }
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
}
