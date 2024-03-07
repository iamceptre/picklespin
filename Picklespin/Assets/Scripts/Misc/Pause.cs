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
                isPaused = true;
                PauseGame();
            }
            else
            {
                isPaused = false;
                UnpauseGame();
            }
        }
    }

    private void PauseGame()
    {
        pauseEvent.Invoke();
        timescaleBeforePausing = Time.timeScale;
        Time.timeScale = 0;
    }

    private void UnpauseGame()
    {
        Time.timeScale = timescaleBeforePausing;
        unpauseEvent.Invoke();
    }

}
