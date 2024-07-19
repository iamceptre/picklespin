using UnityEngine;

public class EnableLookAtOnEnter : MonoBehaviour
{

    [SerializeField] private LookAtPlayer lookAtScript;

    private void Start()
    {
        lookAtScript.enabled = false;
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            lookAtScript.enabled = true;
        }
    }

    */

    /*
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            lookAtScript.DisableWhenFarAway();
        }
    }

    */

}
