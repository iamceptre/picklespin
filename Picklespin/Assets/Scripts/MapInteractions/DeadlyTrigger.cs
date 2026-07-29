using UnityEngine;

public class DeadlyTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Death.instance.PlayerDeath();
        }
    }
}
