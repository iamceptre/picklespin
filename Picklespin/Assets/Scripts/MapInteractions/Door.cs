using UnityEngine;
using DG.Tweening;
using FMODUnity;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform _transform;
    [SerializeField] private StudioEventEmitter doorOpenSound;
    [SerializeField] private StudioEventEmitter doorCloseSound;
    [SerializeField] private StudioEventEmitter doorLockedSound;

    private static readonly Vector3 rotationVector = new(0, 0, 90);
    private const float animationTime = 0.8f;
    private const float maxDistance = 7f;
    private const float fallbackDistance = 3f;             // within this, aim is ignored entirely
    private const float maxDistanceSqr = maxDistance * maxDistance;
    private const float fallbackDistanceSqr = fallbackDistance * fallbackDistance;
    private const float aimRadius = 0.25f;

    private static readonly List<Door> doorsInRange = new();
    private static readonly RaycastHit[] aimHits = new RaycastHit[8];

    private static Door resolvedTarget;
    private static int resolvedFrame = -1;
    private static bool crosshairHeld;
    private static bool tipShown;

    private static bool initialized;
    private static Transform mainCamera;
    private static Animator handAnimator;
    private static TipManager tipManager;
    private static CrosshairManager crosshair;

    [Header("Logic")]
    public bool isLocked;
    private bool isOpened;
    private bool canButtonBuffer = true;

    [Header("References")]
    [Tooltip("the solid (non-trigger) collider — auto-found if left empty")]
    [SerializeField] private Collider myCollider;
    [SerializeField] private InputActionReference interactAction;

    private Vector3 startRot;
    private float sqrDistanceToPlayer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        doorsInRange.Clear();
        resolvedTarget = null;
        resolvedFrame = -1;
        crosshairHeld = false;
        tipShown = false;

        initialized = false;
        mainCamera = null;
        handAnimator = null;
        tipManager = null;
        crosshair = null;
    }

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
    private static void EnsureInitialized()
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

    private void Update()
    {
        CurrentTarget();
        if (sqrDistanceToPlayer > maxDistanceSqr) LeaveRange();
    }

    private void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        if (!canButtonBuffer) return;
        canButtonBuffer = false;

        if (CurrentTarget() == this) Interact();
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        canButtonBuffer = true;
    }

    // every door in range asks this every frame, and so does the press - resolving once
    // costs one sweep a frame instead of one per door
    private static Door CurrentTarget()
    {
        if (resolvedFrame == Time.frameCount) return resolvedTarget;
        resolvedFrame = Time.frameCount;

        resolvedTarget = ResolveTarget();
        UpdateFeedback();
        return resolvedTarget;
    }

    // re-read rather than latched on the transition: a door can be unlocked while the
    // player already stands at it, and the tip has to start offering itself on its own
    private static void UpdateFeedback()
    {
        bool wantsCrosshair = resolvedTarget;
        if (wantsCrosshair != crosshairHeld)
        {
            crosshairHeld = wantsCrosshair;
            if (crosshair)
            {
                if (wantsCrosshair) crosshair.ShowCrosshair(); else crosshair.HideCrosshair();
            }
        }

        bool wantsTip = resolvedTarget && !resolvedTarget.isLocked;
        if (wantsTip != tipShown)
        {
            tipShown = wantsTip;
            if (tipManager)
            {
                if (wantsTip) tipManager.Show(0); else tipManager.Hide(0);
            }
        }
    }

    private static Door ResolveTarget()
    {
        if (!mainCamera) return null;

        Door aimed = null, nearest = null;
        float bestAimDistance = float.MaxValue, bestNearDistance = float.MaxValue;

        foreach (Door door in doorsInRange)
        {
            if (!door.myCollider) continue;

            float sqrDistance = door.MeasureDistance();
            if (sqrDistance <= fallbackDistanceSqr && sqrDistance < bestNearDistance)
            {
                bestNearDistance = sqrDistance;
                nearest = door;
            }

            if (door.IsUnderCrosshair(out float aimDistance) && aimDistance < bestAimDistance)
            {
                bestAimDistance = aimDistance;
                aimed = door;
            }
        }

        return aimed ? aimed : nearest;
    }

    // stamped so the range check can read what the resolve already measured
    private float MeasureDistance()
    {
        Vector3 camPos = mainCamera.position;
        // ClosestPoint returns the query point itself when it is inside the
        // collider, which yields 0 — exactly the "standing right at it" case
        sqrDistanceToPlayer = (myCollider.ClosestPoint(camPos) - camPos).sqrMagnitude;
        return sqrDistanceToPlayer;
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
            if (isOpened) CloseDoor(); else OpenDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        EnsureInitialized();
        if (!doorsInRange.Contains(this)) doorsInRange.Add(this);
        sqrDistanceToPlayer = 0f;
        enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) LeaveRange();
    }

    private void OnDestroy()
    {
        Forget();
    }

    private void LeaveRange()
    {
        Forget();
        enabled = false;
    }

    // the feedback is only ever refreshed by a door still in range, so the last one out
    // has to hand it back itself
    private void Forget()
    {
        doorsInRange.Remove(this);
        if (resolvedTarget != this) return;

        resolvedTarget = null;
        resolvedFrame = -1;
        UpdateFeedback();
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
