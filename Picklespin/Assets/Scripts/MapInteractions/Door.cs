using UnityEngine;
using DG.Tweening;
using FMODUnity;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

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
    private static readonly float animationTime = PhiMath.PHI * 0.5f; // ≈ 0.809s
    private static readonly float maxDistance = PhiMath.PHI4;         // ≈ 6.85m
    [SerializeField, Tooltip("interact works without looking directly at the door within this distance")]
    private float fallbackInteractDistance = PhiMath.PHI3; // ≈ 4.24m

    private static readonly List<Door> doorsInRange = new();

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

        if (Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == myCollider)
            {
                Interact();
                return;
            }
            foreach (Door door in doorsInRange)
            {
                if (door.myCollider == hit.collider) return; // that door's own handler responds
            }
        }

        // fallback: nothing door-like under the crosshair, closest in-range door interacts
        if (GetClosestFallbackDoor() == this) Interact();
    }

    private void Interact()
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

    private bool IsFallbackEligible()
    {
        Vector3 toDoor = myCollider.ClosestPoint(mainCamera.position) - mainCamera.position;
        if (toDoor.sqrMagnitude > fallbackInteractDistance * fallbackInteractDistance) return false;
        return Vector3.Dot(mainCamera.forward, toDoor.normalized) > 0f; // door is in front of the player
    }

    private static Door GetClosestFallbackDoor()
    {
        Door closest = null;
        float bestSqrDistance = float.MaxValue;
        foreach (Door door in doorsInRange)
        {
            if (!door.IsFallbackEligible()) continue;
            float sqrDistance = (door.myCollider.ClosestPoint(door.mainCamera.position) - door.mainCamera.position).sqrMagnitude;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                closest = door;
            }
        }
        return closest;
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
            if (!doorsInRange.Contains(this)) doorsInRange.Add(this);
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
            doorsInRange.Remove(this);
            tipManager.Hide(0);
            crosshair.HideCrosshair();
            enabled = false;
        }
    }

    private void OnDestroy()
    {
        doorsInRange.Remove(this);
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
                doorsInRange.Remove(this);
                tipManager.Hide(0);
                crosshair.HideCrosshair();
                enabled = false;
                yield break;
            }
            bool isLookingAtDoor =
                (Physics.Raycast(mainCamera.position, mainCamera.forward, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore)
                && hit.collider == myCollider)
                || GetClosestFallbackDoor() == this;
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
