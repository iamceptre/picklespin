using UnityEngine;
using UnityEngine.Events;

public class Pause : MonoBehaviour
{

    private float timescaleBeforePausing;
    private bool isPaused;

    [SerializeField] private UnityEvent pauseEvent;
    [SerializeField] private UnityEvent unpauseEvent;

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
    }

    public void UnpauseGame()
    {
        Time.timeScale = timescaleBeforePausing;
        unpauseEvent.Invoke();
        isPaused = false;
    }

}
