using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MenuScreen))]
public class Pause : MonoBehaviour
{
    public static Pause instance { get; private set; }

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("References")]
    [SerializeField, Tooltip("AudioSnapshotManager key held while paused - the short key it is registered under, not the FMOD path; empty means no snapshot")]
    private string snapshotKey = "Pause";

    private MenuScreen screen;
    private float timeScaleBeforePausing = 1f;
    private Coroutine cursorReapply;

    public bool IsPaused { get; private set; }

    public static bool CanOpen => !PauseGate.Blocked;

    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;

        screen = MenuScreen.Of(GetComponent<Canvas>());
        if (!screen)
        {
            DevLog.Error($"{nameof(Pause)}: has to sit on the pause menu's own Canvas. Pausing stays off until it does.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        pauseAction.action.performed += OnPausePerformed;
        pauseAction.action.Enable();
    }

    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePerformed;
        pauseAction.action.Disable();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        PauseGate.Release(this);
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        // a page deeper in backs out first, the way its own back button would
        if (IsPaused && SettingsScreen.Instance && SettingsScreen.Instance.IsOpen)
        {
            SettingsScreen.Instance.Back();
        }
        else if (IsPaused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }

        // Escape breaks the cursor lock on its own, later in this same frame, so the state the
        // lock just asked for has to be asked for again once the frame is over
        if (cursorReapply != null) StopCoroutine(cursorReapply);
        cursorReapply = StartCoroutine(ReapplyCursorNextFrame());
    }

    private IEnumerator ReapplyCursorNextFrame()
    {
        yield return null;

        PlayerControlLock.ReapplyCursor();
        cursorReapply = null;
    }

    public void PauseGame()
    {
        if (IsPaused || PauseGate.Blocked) return;

        IsPaused = true;
        timeScaleBeforePausing = Time.timeScale;
        Time.timeScale = 0f;

        PauseGate.Block(this);
        PlayerControlLock.Set(this, true);
        SetSnapshot(true);
        screen.Open();
        ClearSelection();
    }

    public void UnpauseGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = timeScaleBeforePausing;

        MenuScreen.CancelPendingSteps();
        if (SettingsScreen.Instance) SettingsScreen.Instance.CloseImmediate();
        screen.Close();
        SetSnapshot(false);
        PlayerControlLock.Set(this, false);
        PauseGate.Release(this);
        ClearSelection();
    }

    // the portal closed on the player: the run is over, so the clock stops and the controls go
    // away, but there is no menu to open and no way back out of it
    public void PauseGamePortalClosedFail()
    {
        if (IsPaused) UnpauseGame();

        Time.timeScale = 0f;
        PauseGate.Block(this);
        PlayerControlLock.Set(this, true);
        ClearSelection();
    }

    private void SetSnapshot(bool held)
    {
        if (string.IsNullOrEmpty(snapshotKey) || !AudioSnapshotManager.Instance) return;

        if (held) AudioSnapshotManager.Instance.EnableSnapshot(snapshotKey);
        else AudioSnapshotManager.Instance.DisableSnapshot(snapshotKey);
    }

    private static void ClearSelection()
    {
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
    }
}
