using UnityEngine;

public class PlayTimerOnlyHere : MonoBehaviour
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
        if (roundSystem && other.gameObject.CompareTag("Player")) roundSystem.ReportPlayerInArena();
    }

}
