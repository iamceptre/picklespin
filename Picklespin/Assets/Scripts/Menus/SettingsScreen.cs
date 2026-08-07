using UnityEngine;

// The one settings page, wherever it is dropped: the main menu, the pause menu, a future level.
// It knows nothing about who opened it - MenuScreen remembers that - so the prefab needs no
// per-scene wiring, and it writes and applies what was changed the moment it closes.
[RequireComponent(typeof(MenuScreen))]
public class SettingsScreen : MonoBehaviour
{
    public static SettingsScreen Instance { get; private set; }

    private MenuScreen screen;
    private BulkSettingsManager saver;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            DevLog.Warn($"{nameof(SettingsScreen)}: this scene has more than one, the first one keeps the job", this);
            enabled = false;
            return;
        }
        Instance = this;

        screen = MenuScreen.Of(GetComponent<Canvas>());
        saver = GetComponent<BulkSettingsManager>();

        if (!screen)
        {
            DevLog.Error($"{nameof(SettingsScreen)}: has to sit on the settings page's own Canvas", this);
            enabled = false;
            return;
        }

        screen.Closed += Apply;
    }

    private void Start() => BindCamera();

    private void OnDestroy()
    {
        if (screen) screen.Closed -= Apply;
        if (Instance == this) Instance = null;
    }

    public bool IsOpen => screen && screen.IsOpen;

    public void Open(MenuScreen from) => MenuScreen.Step(from, screen);

    public void Back() => MenuScreen.StepBack(screen);

    // the menu behind it is going away, so there is nothing to step back to
    public void CloseImmediate()
    {
        if (!IsOpen) return;

        screen.ApplyImmediate(false);
        Apply();
    }

    // dropped into a level, the screen has to render through whatever camera that level draws
    // its UI with - the one it was authored against belongs to another scene
    private void BindCamera()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.worldCamera) return;

        canvas.worldCamera = Camera.main;
    }

    // the sliders write PlayerPrefs as they are dragged and the live ones already went through
    // ComfortSettings on the way; these are the two that only take effect when asked
    private void Apply()
    {
        if (saver) saver.SaveAllSettings();
        PlayerPrefs.Save();

        if (FPSLimit.instance)
        {
            FPSLimit.instance.framerateLimit = PlayerPrefs.GetFloat(SettingsDefaults.FramerateLimitKey, SettingsDefaults.FramerateLimit);
            FPSLimit.instance.SetFramerate();
        }

        if (MouselookXY.instance) MouselookXY.instance.RestoreSensitivity();
    }
}
