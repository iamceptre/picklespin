using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Pickupable_Item : MonoBehaviour
{

    [SerializeField] private UnityEvent pickupEvent;
    private Light myLight;
    private Collider myCollider;
    private float startingYPos;
    private float myLightIntensitivity;
    private Renderer rend;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myLight = GetComponent<Light>();
        rend = GetComponent<Renderer>();
        particle = GetComponent<ParticleSystem>();

        if (particle != null) {
            emission = particle.emission;
        }

        if (myLight != null)
            {
                myLightIntensitivity = myLight.intensity;
            }
    }

    private void Start()
    {
        startingYPos = transform.position.y;
        PickupableAnimation();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Pickup();
        }
    }

    private void PickupableAnimation()
    {
        transform.DOMoveY(startingYPos + 0.3f, 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        transform.DORotate(new Vector3(360, 360, 0), 1).SetLoops(-1);
        myLight.DOIntensity(myLightIntensitivity * 0.5f, 1).SetLoops(-1, LoopType.Yoyo);
    }

    private void FadeOut()
    {
        rend.enabled = false;
        if (particle != null)
        {
            emission.enabled = false;
        }

        myLight.DOIntensity(0, 0.5f).OnComplete(DestroyMe);

    }

    private void DestroyMe()
    {
        Destroy(gameObject);
    }

    private void Pickup()
    {
        pickupEvent.Invoke();
        transform.DOKill();
        myLight.DOKill();
        myCollider.enabled = false;
        FadeOut();
    }


}
