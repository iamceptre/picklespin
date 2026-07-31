using UnityEngine;

// Resuming is deliberately not this script's job - only PlayTimerOnlyHere starts the
// clock again, or healing an angel would restart the round mid wish menu.
public class PauseTimerOnEnter : MonoBehaviour
{

    private RoundSystem roundSystem;

    private void Start()
    {
        roundSystem = RoundSystem.instance;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            roundSystem.isCounting = false;
        }
    }

}
