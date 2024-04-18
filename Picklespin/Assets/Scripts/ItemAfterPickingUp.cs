using DG.Tweening;
using UnityEngine;


public class ItemAfterPickingUp : MonoBehaviour
{

    private Light myLight;
    private Collider myCollider;
    private Renderer rend;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myLight = GetComponent<Light>();
        rend = GetComponent<Renderer>();
        particle = GetComponent<ParticleSystem>();

        if (particle != null)
        {
            emission = particle.emission;
        }
    }

    public void Pickup()
    {
        transform.DOKill();
        myLight.DOKill();
        myCollider.enabled = false;
        FadeOut();
    }


    private void FadeOut()
    {
        rend.enabled = false;
        if (particle != null)
        {
            emission.enabled = false;
        }

        if (myLight != null)
        {
            myLight.DOIntensity(0, 0.5f).OnComplete(DestroyMe);
        }
        else
        {
            DestroyMe();
        }
    }

    private void DestroyMe()
    {
        Destroy(gameObject);
    }
}
