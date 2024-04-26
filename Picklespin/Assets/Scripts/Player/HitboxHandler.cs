using UnityEngine;

public class HitboxHandler : MonoBehaviour
{
    private Death death;
    private void Start()
    {
        death = Death.instance; 
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EvilEntity"))
        {
            death.PlayerDeath(); //change it so enemies takes your HP from THEIR class
        }
    }

}
