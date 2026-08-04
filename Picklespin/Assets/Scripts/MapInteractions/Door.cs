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
    private const float animationTime = 0.8f;
    private const float maxDistance = 7f;
    private const float fallbackDistance = 3f;             // within this, aim is ignored entirely
    private const float aimRadius = 0.25f;

    private static readonly List<Door> doorsInRange = new();
    private static readonly RaycastHit[] aimHits = new RaycastHit[8];

    [Header("Logic")]
    public bool isLocked;
    private bool isOpened;
    private bool canButtonBuffer = true;
    private bool playerInRange;
    private bool initialized;

    [Header("References")]
    [Tooltip("the solid (non-trigger) collider — auto-found if left empty")]
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

        // the prefabs ship with this unassigned; a door has a solid collider and a
        // trigger volume, and the solid one is the target
        if (!myCollider)
        {
            foreach (Collider c in GetComponentsInChildren<Collider>(true))
            {
                if (c.isTrigger) continue;
                myCollider = c;
                break;
            }
        }
    }

    // the component starts disabled, so Start() would run after the OnTriggerEnter
    // that enabled it - initialisation is explicit instead
    private void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;

        crosshair = CrosshairManager.Instance;
        handAnimator = PublicPlayerHandAnimator.instance._animator;
        mainCamera = CachedCameraMain.instance.cachedTransform;
        tipManager = TipManager.instance;
    }

    private void OnEnable()
    {
        // a press that ended out of range never delivers its "canceled"
        canButtonBuffer = true;

        interactAction.action.started += OnInteractStarted;
        interactAction.action.canceled += OnInteractCanceled;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.started -= OnInteractStarted;
        interactAction.action.canceled -= OnInteractCanceled;
        // never Disable()d: the action asset is shared by every door
    }

    private void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        if (!canButtonBuffer) return;
        canButtonBuffer = false;

        if (ResolveTarget() == this) Interact();
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        canButtonBuffer = true;
    }

    private static Door ResolveTarget()
    {
        Door aimed = null, nearest = null;
        float bestAimDistance = float.MaxValue, bestNearDistance = float.MaxValue;

        foreach (Door door in doorsInRange)
        {
            if (!door.myCollider || !door.mainCamera) continue;

            if (door.IsUnderCrosshair(out float aimDistance) && aimDistance < bestAimDistance)
            {
                bestAimDistance = aimDistance;
                aimed = door;
            }

            float distance = door.DistanceToPlayer();
            if (distance <= fallbackDistance && distance < bestNearDistance)
            {
                bestNearDistance = distance;
                nearest = door;
            }
        }

        return aimed ? aimed : nearest;
    }

    private float DistanceToPlayer()
    {
        Vector3 camPos = mainCamera.position;
        // ClosestPoint returns the query point itself when it is inside the
        // collider, which yields 0 — exactly the "standing right at it" case
        return Vector3.Distance(myCollider.ClosestPoint(camPos), camPos);
    }

    private bool IsUnderCrosshair(out float distance)
    {
        distance = float.MaxValue;

        // SphereCast, not Raycast: a zero-width ray demands pixel-perfect aim
        int count = Physics.SphereCastNonAlloc(mainCamera.position, aimRadius, mainCamera.forward,
            aimHits, maxDistance, layerMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (aimHits[i].collider != myCollider) continue;
            distance = aimHits[i].distance;
            return true;
        }
        return false;
    }

    private void Interact()
    {
        if (isLocked)
        {
            handAnimator.SetTrigger("Hand_Fail");
            if (doorLockedSound) doorLockedSound.Play();
        }
        else
        {
            if (tipManager) tipManager.Hide(0);
            if (isOpened) CloseDoor(); else OpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        EnsureInitialized();
        playerInRange = true;
        if (!doorsInRange.Contains(this)) doorsInRange.Add(this);
        enabled = true;
        if (!isLocked && tipManager) tipManager.Show(0);
        StopAllCoroutines();
        StartCoroutine(CheckDoorRangeAndSight());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) LeaveRange();
    }

    private void OnDestroy()
    {
        doorsInRange.Remove(this);
    }

    private void LeaveRange()
    {
        playerInRange = false;
        doorsInRange.Remove(this);
        if (tipManager) tipManager.Hide(0);
        if (crosshair) crosshair.HideCrosshair();
        enabled = false;
    }

    private IEnumerator CheckDoorRangeAndSight()
    {
        bool wasTargeted = false;
        while (playerInRange)
        {
            //yield return refreshRate;
            yield return null;

            if (DistanceToPlayer() > maxDistance)
            {
                LeaveRange();
                yield break;
            }

            bool isTargeted = ResolveTarget() == this;
            if (isTargeted && !wasTargeted) crosshair.ShowCrosshair();
            else if (!isTargeted && wasTargeted) crosshair.HideCrosshair();
            wasTargeted = isTargeted;
        }
    }

    // state first, then feedback: the emitters ship unassigned on some prefabs, and
    // a throw before isOpened is set leaves a door that can open but never close
    private void OpenDoor()
    {
        isOpened = true;
        _transform.DOKill();
        _transform.DOLocalRotate(startRot + rotationVector, animationTime, RotateMode.Fast);
        handAnimator.SetTrigger("Door_Open");

        if (doorCloseSound) doorCloseSound.Stop();
        if (doorOpenSound) doorOpenSound.Play();
    }

    private void CloseDoor()
    {
        isOpened = false;
        _transform.DOKill();
        _transform.DOLocalRotate(startRot, animationTime, RotateMode.Fast);
        handAnimator.SetTrigger("Door_Close");

        if (doorOpenSound) doorOpenSound.Stop();
        if (doorCloseSound) doorCloseSound.Play();
    }
}
