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

    private static readonly Vector3 rotationVector = new(0, 0, 90);
    private const float animationTime = 0.8f;
    private const float maxDistance = 7f;
    private const float fallbackDistance = 3f;             // within this, aim is ignored entirely
    private const float aimRadius = 0.25f;

    private static readonly List<Door> doorsInRange = new();
    private static readonly RaycastHit[] aimHits = new RaycastHit[8];

    private static Door resolvedTarget;
    private static int resolvedFrame = -1;
    private static Door tipOwner;

    [Header("Logic")]
    public bool isLocked;
    private bool isOpened;
    private bool canButtonBuffer = true;
    private bool playerInRange;
    private bool isTargeted;
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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        doorsInRange.Clear();
        resolvedTarget = null;
        resolvedFrame = -1;
        tipOwner = null;
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

        if (CurrentTarget() == this) Interact();
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        canButtonBuffer = true;
    }

    // every door in range asks this every frame, and the press asks it too - resolving
    // once a frame keeps the tip, the crosshair and the keypress on the same answer
    private static Door CurrentTarget()
    {
        if (resolvedFrame == Time.frameCount) return resolvedTarget;
        resolvedFrame = Time.frameCount;
        resolvedTarget = ResolveTarget();
        return resolvedTarget;
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
        ReleaseTip();
        if (resolvedTarget == this) resolvedTarget = null;
    }

    private void LeaveRange()
    {
        playerInRange = false;
        doorsInRange.Remove(this);
        ReleaseTip();
        SetTargeted(false);
        enabled = false;
    }

    private IEnumerator CheckDoorRangeAndSight()
    {
        while (playerInRange)
        {
            if (DistanceToPlayer() > maxDistance)
            {
                LeaveRange();
                yield break;
            }

            bool targeted = CurrentTarget() == this;
            SetTargeted(targeted);
            UpdateTip(targeted && !isLocked);

            yield return null;
        }
    }

    // the crosshair counts its users, so a door may only ever take one count back out
    private void SetTargeted(bool targeted)
    {
        if (targeted == isTargeted) return;
        isTargeted = targeted;

        if (!crosshair) return;
        if (targeted) crosshair.ShowCrosshair(); else crosshair.HideCrosshair();
    }

    // one tip, many doors: only the door showing it may hide it again
    private void UpdateTip(bool wanted)
    {
        if (wanted)
        {
            if (tipOwner == this) return;
            tipOwner = this;
            if (tipManager) tipManager.Show(0);
            return;
        }

        ReleaseTip();
    }

    private void ReleaseTip()
    {
        if (tipOwner != this) return;
        tipOwner = null;
        if (tipManager) tipManager.Hide(0);
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
