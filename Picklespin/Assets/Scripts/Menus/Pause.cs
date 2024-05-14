using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Pause : MonoBehaviour
{
    public static Pause instance { get; private set; }

    private float timescaleBeforePausing;
    private bool isPaused;

    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private UnityEvent pauseEvent;
    [SerializeField] private UnityEvent unpauseEvent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                UnpauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseEvent.Invoke();
        timescaleBeforePausing = Time.timeScale;
        Time.timeScale = 0;
        isPaused = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void UnpauseGame()
    {
        Time.timeScale = timescaleBeforePausing;
        unpauseEvent.Invoke();
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
