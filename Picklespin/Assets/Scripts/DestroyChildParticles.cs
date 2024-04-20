using UnityEngine;

public class DestroyChildParticles : MonoBehaviour
{

    private ParticleSystem child;


    public void Destroy()
    {
        child = gameObject.GetComponentInChildren<ParticleSystem>(); 
        if (child != null)
        {
            Destroy(child.gameObject);
        }
    }

}
