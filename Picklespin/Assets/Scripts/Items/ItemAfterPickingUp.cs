using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;


public class ItemAfterPickingUp : MonoBehaviour
{

    private Light myLight;
    private float myLightIntestivity;
    private Collider myCollider;
    private Renderer rend;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    [SerializeField] private UnityEvent afterPickingUpEvent;

    [SerializeField] private bool isObjectPooled = true;

    [SerializeField] private bool deactivateAfterPickup = false;

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

        myLightIntestivity = myLight.intensity;
    }

    public void Pickup()
    {
        transform.DOKill();
        myLight.DOKill();
        myCollider.enabled = false;
        FadeOut();
    }

    private void OnEnable()
    {
        if (!myCollider.enabled) {
            myCollider.enabled = true;
            FadeIn();
        }
    }

    private void FadeIn()
    {
        rend.enabled = true;


        if (particle != null)
        {
            emission.enabled = true;
        }

        if (myLight != null)
        {
            myLight.DOIntensity(myLightIntestivity, 0.5f);
        }

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
        if (!isObjectPooled)
        {
            Destroy(gameObject);
            return;
        }

        if (deactivateAfterPickup)
        {
          gameObject.SetActive(false);
        }

        afterPickingUpEvent.Invoke();
    }
}
