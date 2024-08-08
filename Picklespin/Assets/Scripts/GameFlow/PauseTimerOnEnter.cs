using System.Collections;
using UnityEngine;

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


    private void OnDisable()
    {
        roundSystem.isCounting = true;
    }

}
