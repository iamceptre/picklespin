using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteractionRunner : MonoBehaviour
{
    private const float maxDistanceSqr = Door.MaxDistance * Door.MaxDistance;
    private const float fallbackDistanceSqr = Door.FallbackDistance * Door.FallbackDistance;

    private static readonly List<Door> allDoors = new();

    private static DoorInteractionRunner host;

    private static int resolvedFrame = -1;
    private static bool crosshairHeld;
    private static bool tipShown;

    private static Transform mainCamera;
    private static Animator handAnimator;
    private static TipManager tipManager;
    private static CrosshairManager crosshair;

    private static InputAction boundInteract;

    public static IReadOnlyList<Door> All => allDoors;
    public static Door Target { get; private set; }

    public static bool InputBound => boundInteract != null;
    public static bool InputEnabled => boundInteract != null && boundInteract.enabled;
    public static int InputReEnables { get; private set; }

    public static bool HasCamera => mainCamera;
    public static bool HasHandAnimator => handAnimator;

    public static Door LastPressDoor { get; private set; }
    public static Door.PressResult LastPress { get; private set; }
    public static int LastPressFrame { get; private set; } = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        allDoors.Clear();
        host = null;

        resolvedFrame = -1;
        crosshairHeld = false;
        tipShown = false;

        mainCamera = null;
        handAnimator = null;
        tipManager = null;
        crosshair = null;

        boundInteract = null;
        InputReEnables = 0;

        Target = null;
        LastPressDoor = null;
        LastPressFrame = -1;
    }

    public static void Register(Door door)
    {
        if (!allDoors.Contains(door)) allDoors.Add(door);

        BindInput(door.InteractAction);
        Ensure();
    }

    public static void Unregister(Door door)
    {
        allDoors.Remove(door);
        if (Target != door) return;

        Target = null;
        resolvedFrame = -1;
        UpdateFeedback();
    }

    // doors ship with their component disabled and own no Update of their own - one host
    // ticks the whole set, so a door can never be left out of the sweep
    private static void Ensure()
    {
        if (host) return;

        GameObject go = new(nameof(DoorInteractionRunner)) { hideFlags = HideFlags.HideInHierarchy };
        DontDestroyOnLoad(go);
        host = go.AddComponent<DoorInteractionRunner>();
    }

    private void Update()
    {
        if (host != this) return;

        Resolve();
    }

    private void OnDestroy()
    {
        if (host != this) return;

        host = null;
        UnbindInput();
    }

    private static void BindInput(InputActionReference reference)
    {
        if (boundInteract != null || !reference || reference.action == null) return;

        boundInteract = reference.action;
        boundInteract.started += OnInteractStarted;
        boundInteract.Enable();
    }

    private static void UnbindInput()
    {
        if (boundInteract == null) return;

        boundInteract.started -= OnInteractStarted;
        // never Disable()d: the action asset is shared
        boundInteract = null;
    }

    // one sweep a frame for every door in the scene. no physics and no trigger volumes:
    // a trigger swings away with the leaf, and a door dropped while the player still stood
    // inside it never got its enter callback again
    private static void Resolve()
    {
        if (resolvedFrame == Time.frameCount) return;
        resolvedFrame = Time.frameCount;

        if (!EnsureInitialized()) return;

        if (boundInteract != null && !boundInteract.enabled)
        {
            boundInteract.Enable();
            InputReEnables++;
        }

        Vector3 origin = mainCamera.position;
        Vector3 forward = mainCamera.forward;

        Door aimed = null, nearest = null;
        float bestAimDistance = float.MaxValue, bestNearDistance = float.MaxValue;

        for (int i = 0; i < allDoors.Count; i++)
        {
            Door door = allDoors[i];

            float sqrDistance = door.MeasureDistance(origin);
            if (sqrDistance > maxDistanceSqr) continue;

            if (sqrDistance <= fallbackDistanceSqr && sqrDistance < bestNearDistance)
            {
                bestNearDistance = sqrDistance;
                nearest = door;
            }

            if (!door.IsUnderCrosshair(forward) || door.AimDistance >= bestAimDistance) continue;

            bestAimDistance = door.AimDistance;
            aimed = door;
        }

        Target = aimed ? aimed : nearest;
        UpdateFeedback();
    }

    // re-resolved rather than latched: a stale reference to a destroyed HUD or camera
    // used to take the whole interaction down for the rest of the run
    private static bool EnsureInitialized()
    {
        if (!mainCamera)
        {
            CachedCameraMain camera = CachedCameraMain.instance;
            mainCamera = camera ? camera.cachedTransform : null;
            if (!mainCamera) return false;
        }

        if (!crosshair) crosshair = CrosshairManager.Instance;
        if (!tipManager) tipManager = TipManager.instance;
        if (!handAnimator)
        {
            PublicPlayerHandAnimator hands = PublicPlayerHandAnimator.instance;
            handAnimator = hands ? hands._animator : null;
        }

        return true;
    }

    // re-read rather than latched on the transition: a door can be unlocked while the
    // player already stands at it, and the tip has to start offering itself on its own
    private static void UpdateFeedback()
    {
        bool wantsCrosshair = Target;
        if (wantsCrosshair != crosshairHeld && crosshair)
        {
            crosshairHeld = wantsCrosshair;
            if (wantsCrosshair) crosshair.ShowCrosshair(); else crosshair.HideCrosshair();
        }

        bool wantsTip = Target && !Target.isLocked;
        if (wantsTip != tipShown && tipManager)
        {
            tipShown = wantsTip;
            if (wantsTip) tipManager.Show(0); else tipManager.Hide(0);
        }
    }

    private static void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        Resolve();

        Door door = Target;
        if (!door)
        {
            RecordPress(null, default);
            return;
        }

        Door.PressResult result = door.Interact(handAnimator);
        RecordPress(door, result);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void RecordPress(Door door, Door.PressResult result)
    {
        LastPressDoor = door;
        LastPress = result;
        LastPressFrame = Time.frameCount;
    }
}
