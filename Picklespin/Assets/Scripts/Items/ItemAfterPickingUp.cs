using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;


public class ItemAfterPickingUp : MonoBehaviour
{

    private Light myLight;
    private Collider myCollider;
    private Renderer rend;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    [SerializeField] private UnityEvent afterPickingUpEvent; //ACTUALLY THE SHIT

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myLight = GetComponent<Light>();
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
        }
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
        afterPickingUpEvent.Invoke();
    }


    private void FadeOut()
    {

        if (rend != null)
        {
            rend.enabled = false;
        }


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
