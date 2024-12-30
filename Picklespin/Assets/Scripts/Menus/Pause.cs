using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    private SnapshotManager snapshotManager;
    public static Pause instance { get; private set; }
    private float timescaleBeforePausing;
    private bool isPaused;
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private UnityEvent pauseEvent;
    [SerializeField] private UnityEvent unpauseEvent;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pauseAction;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    private void Start()
    {
        snapshotManager = SnapshotManager.instance;
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

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (!isPaused) PauseGame(); else UnpauseGame();
    }

    public void PauseGame()
    {
        System.GC.Collect();
        pauseEvent.Invoke();
        snapshotManager.Pause.Play();
        timescaleBeforePausing = Time.timeScale;
        Time.timeScale = 0;
        isPaused = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void UnpauseGame()
    {
        System.GC.Collect();
        unpauseEvent.Invoke();
        snapshotManager.Pause.Stop();
        Time.timeScale = timescaleBeforePausing;
        isPaused = false;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void PauseGamePortalClosedFail()
    {
        pauseEvent.Invoke();
        PauseMenu.SetActive(false);
        timescaleBeforePausing = Time.timeScale;
        Time.timeScale = 0;
        isPaused = true;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
