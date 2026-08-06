using UnityEngine;

// Resuming is deliberately not this script's job - only PlayTimerOnlyHere starts the
// clock again, or healing an angel would restart the round mid wish menu. Both volumes
// report every frame the player is in them rather than latching once, so a missed
// crossing cannot leave the angel area stuck on; where the two overlap this one wins.
public class PauseTimerOnEnter : MonoBehaviour
{

    private RoundSystem roundSystem;

    private void Start()
    {
        roundSystem = RoundSystem.instance;
    }

    private void OnTriggerEnter(Collider other) => Report(other);

    private void OnTriggerStay(Collider other) => Report(other);

    private void Report(Collider other)
    {
        if (roundSystem && other.gameObject.CompareTag("Player")) roundSystem.ReportPlayerInAngelArea();
    }

}
