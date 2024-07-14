using UnityEngine;

public class DeadlyTrigger : MonoBehaviour
{

    private Death death;

    private void Awake()
    {
        death = Death.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("umarlo od tirggera");

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("umarles od tirggera");
            //death.PlayerDeath();
        }
    }


}
