using UnityEngine;
using DG.Tweening;
using FMODUnity;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour
{
    public enum PressResult { Opened, Closed, Locked }

    public const float MaxDistance = 7f;
    public const float FallbackDistance = 3f;

    [Header("Parameters")]
    [SerializeField] private Transform _transform;
    [SerializeField] private StudioEventEmitter doorOpenSound;
    [SerializeField] private StudioEventEmitter doorCloseSound;
    [SerializeField] private StudioEventEmitter doorLockedSound;

    private static readonly Vector3 rotationVector = new(0, 0, 90);
    private const float animationTime = 0.8f;
    private const float aimRadius = 0.25f;
    private const float framePadding = 0.15f;
    private const float parallelEpsilon = 1e-5f;

    private static readonly int handFailHash = Animator.StringToHash("Hand_Fail");
    private static readonly int doorOpenHash = Animator.StringToHash("Door_Open");
    private static readonly int doorCloseHash = Animator.StringToHash("Door_Close");

    [Header("Logic")]
    public bool isLocked;
    private bool isOpened;

    [Header("References")]
    [Tooltip("the solid (non-trigger) collider — auto-found if left empty. Its pose at startup is the interaction hitbox, and that hitbox stays in the frame when the door swings")]
    [SerializeField] private Collider myCollider;
    [SerializeField] private InputActionReference interactAction;

    private Vector3 startRot;

    private Vector3 frameCenter;
    private Quaternion frameRotation;
    private Quaternion frameInverseRotation;
    private Vector3 frameHalfExtents;
    private Vector3 aimHalfExtents;

    private Vector3 localOrigin;
    private float sqrDistanceToPlayer = float.MaxValue;
    private float aimDistance = -1f;

    public InputActionReference InteractAction => interactAction;

    public bool IsOpen => isOpened;
    public float SqrDistanceToPlayer => sqrDistanceToPlayer;
    public float AimDistance => aimDistance;
    public Collider Leaf => myCollider;
    public Vector3 FrameCenter => frameCenter;
    public Quaternion FrameRotation => frameRotation;
    public Vector3 FrameHalfExtents => frameHalfExtents;

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

        CaptureFrame();
        DoorInteractionRunner.Register(this);
    }

    private void OnDestroy() => DoorInteractionRunner.Unregister(this);

    // the leaf swings, the frame does not: the pose the collider holds at startup is
    // stamped into an oriented box here and every later query runs against that box,
    // so an open door is still grabbed where the doorway is
    private void CaptureFrame()
    {
        if (!myCollider)
        {
            frameCenter = _transform.position;
            frameRotation = _transform.rotation;
            frameHalfExtents = new Vector3(framePadding, framePadding, framePadding);
            DevLog.Error($"{name}: no solid collider — the door has no interaction hitbox.", this);
        }
        else if (myCollider is BoxCollider box)
        {
            Transform colliderTransform = myCollider.transform;
            Vector3 scale = colliderTransform.lossyScale;

            frameCenter = colliderTransform.TransformPoint(box.center);
            frameRotation = colliderTransform.rotation;
            frameHalfExtents = new Vector3(
                Mathf.Abs(box.size.x * scale.x) * 0.5f + framePadding,
                Mathf.Abs(box.size.y * scale.y) * 0.5f + framePadding,
                Mathf.Abs(box.size.z * scale.z) * 0.5f + framePadding);
        }
        else
        {
            Bounds bounds = myCollider.bounds;
            frameCenter = bounds.center;
            frameRotation = Quaternion.identity;
            frameHalfExtents = bounds.extents + new Vector3(framePadding, framePadding, framePadding);
        }

        frameInverseRotation = Quaternion.Inverse(frameRotation);
        aimHalfExtents = frameHalfExtents + new Vector3(aimRadius, aimRadius, aimRadius);
    }

    // squared distance from the camera to the surface of the frame box. rotation preserves
    // length, so the whole comparison stays in the box's own space, and the rotated origin
    // is kept for the aim test that follows it in the same sweep
    public float MeasureDistance(Vector3 point)
    {
        localOrigin = frameInverseRotation * (point - frameCenter);
        aimDistance = -1f;

        float x = localOrigin.x - Mathf.Clamp(localOrigin.x, -frameHalfExtents.x, frameHalfExtents.x);
        float y = localOrigin.y - Mathf.Clamp(localOrigin.y, -frameHalfExtents.y, frameHalfExtents.y);
        float z = localOrigin.z - Mathf.Clamp(localOrigin.z, -frameHalfExtents.z, frameHalfExtents.z);

        sqrDistanceToPlayer = x * x + y * y + z * z;
        return sqrDistanceToPlayer;
    }

    // slab test against the frame box grown by the aim radius — the analytic form of the
    // SphereCast this used to fire, minus the shared hit buffer that could drop the door
    public bool IsUnderCrosshair(Vector3 direction)
    {
        Vector3 localDirection = frameInverseRotation * direction;

        float near = 0f, far = MaxDistance;

        if (!Slab(localOrigin.x, localDirection.x, aimHalfExtents.x, ref near, ref far)) return false;
        if (!Slab(localOrigin.y, localDirection.y, aimHalfExtents.y, ref near, ref far)) return false;
        if (!Slab(localOrigin.z, localDirection.z, aimHalfExtents.z, ref near, ref far)) return false;

        aimDistance = near;
        return true;
    }

    private static bool Slab(float start, float step, float half, ref float near, ref float far)
    {
        if (Mathf.Abs(step) < parallelEpsilon) return start >= -half && start <= half;

        float inverseStep = 1f / step;
        float first = (-half - start) * inverseStep;
        float second = (half - start) * inverseStep;
        if (first > second) (first, second) = (second, first);

        if (first > near) near = first;
        if (second < far) far = second;
        return near <= far;
    }

    public PressResult Interact(Animator handAnimator)
    {
        if (isLocked)
        {
            if (handAnimator) handAnimator.SetTrigger(handFailHash);
            if (doorLockedSound) doorLockedSound.Play();
            return PressResult.Locked;
        }

        if (isOpened)
        {
            CloseDoor(handAnimator);
            return PressResult.Closed;
        }

        OpenDoor(handAnimator);
        return PressResult.Opened;
    }

    // state first, then feedback: the emitters ship unassigned on some prefabs, and
    // a throw before isOpened is set leaves a door that can open but never close
    private void OpenDoor(Animator handAnimator)
    {
        isOpened = true;
        _transform.DOKill();
        _transform.DOLocalRotate(startRot + rotationVector, animationTime, RotateMode.Fast);
        if (handAnimator) handAnimator.SetTrigger(doorOpenHash);

        if (doorCloseSound) doorCloseSound.Stop();
        if (doorOpenSound) doorOpenSound.Play();
    }

    private void CloseDoor(Animator handAnimator)
    {
        isOpened = false;
        _transform.DOKill();
        _transform.DOLocalRotate(startRot, animationTime, RotateMode.Fast);
        if (handAnimator) handAnimator.SetTrigger(doorCloseHash);

        if (doorOpenSound) doorOpenSound.Stop();
        if (doorCloseSound) doorCloseSound.Play();
    }
}
