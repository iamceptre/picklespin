using UnityEngine;

public class StairGravity : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = PlayerMovement.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerMovement.StairGravity();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerMovement.NormalGravity();
        }
    }
}
