using FMODUnity;
using UnityEngine;

public class DeadlyTrigger : MonoBehaviour
{
    private EventReference sound;
    [SerializeField] private bool playSound = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

            if (playSound)
            {
                RuntimeManager.PlayOneShot(sound);
            }

            Death.instance.PlayerDeath();
        }
    }



}
