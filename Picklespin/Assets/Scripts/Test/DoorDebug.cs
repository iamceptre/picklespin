using UnityEngine;

public class DoorDebug : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private const KeyCode toggleKey = KeyCode.F9;
    private const float listRange = 12f;
    private const float listRangeSqr = listRange * listRange;

    private static readonly Color frameColour = new(0.2f, 1f, 0.35f, 1f);
    private static readonly Color targetColour = new(1f, 0.9f, 0.15f, 1f);
    private static readonly Color lockedColour = new(1f, 0.3f, 0.25f, 1f);
    private static readonly Color leafColour = new(0.45f, 0.55f, 0.75f, 1f);
    private static readonly Color aimColour = new(0.3f, 0.7f, 1f, 1f);

    private static readonly Vector3[] corners = new Vector3[8];
    private static readonly System.Text.StringBuilder line = new(160);

    private static DoorDebug host;

    private bool overlayOn;
    private int reportedFrame = -1;

    private bool warnedNoCamera;
    private bool warnedNoHands;
    private bool warnedNoInput;
    private int reportedReEnables;

    private GUIStyle panelStyle;
    private GUIStyle textStyle;
    private Texture2D panelTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => host = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (host) return;

        GameObject go = new(nameof(DoorDebug)) { hideFlags = HideFlags.HideInHierarchy };
        DontDestroyOnLoad(go);
        host = go.AddComponent<DoorDebug>();

        DevLog.Info($"{nameof(DoorDebug)} armed: {toggleKey} toggles the door overlay, its shapes " +
                    "and its logging. Nothing is logged while it is off.", host);
    }

    private void Update()
    {
        if (host != this) return;

        if (InputCompat.GetKeyDown(toggleKey)) Toggle();
        if (!overlayOn) return;

        ReportPress();
        ReportHealth();
        DrawShapes();
    }

    // switching on re-arms the health warnings and swallows whatever press came last, so
    // the Console reports the state as it is now rather than replaying the quiet stretch
    private void Toggle()
    {
        overlayOn = !overlayOn;
        DevLog.Info($"{nameof(DoorDebug)}: overlay {(overlayOn ? "on" : "off")} ({DoorInteractionRunner.All.Count} doors registered).", this);

        if (!overlayOn) return;

        reportedFrame = DoorInteractionRunner.LastPressFrame;
        warnedNoCamera = false;
        warnedNoHands = false;
        warnedNoInput = false;
    }

    // the one thing worth catching in the wild: a press that resolved to nothing, or to a
    // door other than the one being looked at
    private void ReportPress()
    {
        if (DoorInteractionRunner.LastPressFrame == reportedFrame || DoorInteractionRunner.LastPressFrame < 0) return;
        reportedFrame = DoorInteractionRunner.LastPressFrame;

        Door door = DoorInteractionRunner.LastPressDoor;

        if (!door)
        {
            DevLog.Warn($"{nameof(DoorDebug)}: interact pressed, no door resolved. {NearestReport()}", this);
            return;
        }

        DevLog.Info($"{nameof(DoorDebug)}: interact -> {door.name} = {DoorInteractionRunner.LastPress} ({Verdict(door)}).", door);
    }

    private void ReportHealth()
    {
        if (!DoorInteractionRunner.HasCamera && !warnedNoCamera)
        {
            warnedNoCamera = true;
            DevLog.Error($"{nameof(DoorDebug)}: no {nameof(CachedCameraMain)} - doors cannot resolve a target at all.", this);
        }

        if (!DoorInteractionRunner.HasHandAnimator && !warnedNoHands)
        {
            warnedNoHands = true;
            DevLog.Warn($"{nameof(DoorDebug)}: no hand animator - doors still work, the hand does not react.", this);
        }

        if (!DoorInteractionRunner.InputBound && !warnedNoInput)
        {
            warnedNoInput = true;
            DevLog.Error($"{nameof(DoorDebug)}: the interact action was never bound - no door will ever answer a press.", this);
        }

        if (DoorInteractionRunner.InputReEnables == reportedReEnables) return;
        reportedReEnables = DoorInteractionRunner.InputReEnables;
        DevLog.Warn($"{nameof(DoorDebug)}: the interact action had been disabled by something else and was re-enabled " +
                    $"(#{reportedReEnables}). Doors were dead until this frame.", this);
    }

    // the frame box in green, the swinging leaf in blue: when a door stops answering,
    // the two are supposed to stay apart and the green one is what the player grabs
    private void DrawShapes()
    {
        Door target = DoorInteractionRunner.Target;

        CachedCameraMain camera = CachedCameraMain.instance;
        if (camera && camera.cachedTransform)
        {
            Transform view = camera.cachedTransform;
            Debug.DrawRay(view.position, view.forward * Door.MaxDistance, aimColour);
        }

        for (int i = 0; i < DoorInteractionRunner.All.Count; i++)
        {
            Door door = DoorInteractionRunner.All[i];
            if (door.SqrDistanceToPlayer > listRangeSqr) continue;

            Color colour = door == target ? targetColour : door.isLocked ? lockedColour : frameColour;
            DrawBox(door.FrameCenter, door.FrameRotation, door.FrameHalfExtents, colour);

            if (!door.Leaf) continue;

            Bounds bounds = door.Leaf.bounds;
            DrawBox(bounds.center, Quaternion.identity, bounds.extents, leafColour);
        }
    }

    private static void DrawBox(Vector3 center, Quaternion rotation, Vector3 halfExtents, Color colour)
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 offset = new(
                (i & 1) == 0 ? -halfExtents.x : halfExtents.x,
                (i & 2) == 0 ? -halfExtents.y : halfExtents.y,
                (i & 4) == 0 ? -halfExtents.z : halfExtents.z);
            corners[i] = center + rotation * offset;
        }

        for (int i = 0; i < 8; i++)
        {
            if ((i & 1) == 0) Debug.DrawLine(corners[i], corners[i | 1], colour);
            if ((i & 2) == 0) Debug.DrawLine(corners[i], corners[i | 2], colour);
            if ((i & 4) == 0) Debug.DrawLine(corners[i], corners[i | 4], colour);
        }
    }

    private static string NearestReport()
    {
        Door nearest = null;
        float best = float.MaxValue;

        for (int i = 0; i < DoorInteractionRunner.All.Count; i++)
        {
            Door door = DoorInteractionRunner.All[i];
            if (door.SqrDistanceToPlayer >= best) continue;
            best = door.SqrDistanceToPlayer;
            nearest = door;
        }

        if (!nearest) return "No doors are registered.";

        return $"Nearest is {nearest.name} at {Mathf.Sqrt(best):0.00}m - {Verdict(nearest)}.";
    }

    private static string Verdict(Door door)
    {
        float distance = Mathf.Sqrt(door.SqrDistanceToPlayer);

        if (door.SqrDistanceToPlayer > Door.MaxDistance * Door.MaxDistance)
        {
            return $"out of range, {distance:0.00}m of {Door.MaxDistance}m";
        }

        if (door.AimDistance < 0f && distance > Door.FallbackDistance)
        {
            return $"not under the crosshair and past the {Door.FallbackDistance}m grab range ({distance:0.00}m)";
        }

        if (door == DoorInteractionRunner.Target) return door.isLocked ? "target, locked" : "target";

        return $"in range at {distance:0.00}m but another door won";
    }

    private void OnGUI()
    {
        if (host != this || !overlayOn) return;

        EnsureStyles();

        GUILayout.BeginArea(new Rect(12f, 12f, 560f, Screen.height - 24f), panelStyle);

        line.Clear();
        line.Append("DOORS  registered ").Append(DoorInteractionRunner.All.Count)
            .Append("   input ").Append(DoorInteractionRunner.InputBound ? DoorInteractionRunner.InputEnabled ? "live" : "BOUND BUT DISABLED" : "UNBOUND")
            .Append("   re-enables ").Append(DoorInteractionRunner.InputReEnables);
        GUILayout.Label(line.ToString(), textStyle);

        line.Clear();
        line.Append("camera ").Append(DoorInteractionRunner.HasCamera ? "ok" : "MISSING")
            .Append("   hands ").Append(DoorInteractionRunner.HasHandAnimator ? "ok" : "missing")
            .Append("   target ").Append(DoorInteractionRunner.Target ? DoorInteractionRunner.Target.name : "none");
        GUILayout.Label(line.ToString(), textStyle);

        GUILayout.Space(6f);

        for (int i = 0; i < DoorInteractionRunner.All.Count; i++)
        {
            Door door = DoorInteractionRunner.All[i];
            if (door.SqrDistanceToPlayer > listRangeSqr) continue;

            line.Clear();
            line.Append(door == DoorInteractionRunner.Target ? "> " : "  ").Append(door.name)
                .Append("  ").Append(Mathf.Sqrt(door.SqrDistanceToPlayer).ToString("0.00")).Append('m')
                .Append(door.AimDistance >= 0f ? "  aimed" : "  not aimed")
                .Append(door.isLocked ? "  LOCKED" : string.Empty)
                .Append(door.IsOpen ? "  open" : "  shut")
                .Append("   ").Append(Verdict(door));
            GUILayout.Label(line.ToString(), textStyle);
        }

        GUILayout.EndArea();
    }

    private void EnsureStyles()
    {
        if (panelStyle != null) return;

        panelTexture = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
        panelTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.72f));
        panelTexture.Apply();

        panelStyle = new GUIStyle { padding = new RectOffset(10, 10, 10, 10) };
        panelStyle.normal.background = panelTexture;

        textStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = false };
        textStyle.normal.textColor = Color.white;
    }
#endif
}
