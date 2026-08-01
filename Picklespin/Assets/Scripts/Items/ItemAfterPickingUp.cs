using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class ItemAfterPickingUp : MonoBehaviour
{

    private HandBopAfterItemPickup handBopPickup;
    private Light myLight;
    private float myLightIntestivity;
    private Collider myCollider;
    private Renderer rend;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    [SerializeField] private UnityEvent afterPickingUpEvent;

    [SerializeField] private bool isObjectPooled = true;

    [SerializeField] private bool deactivateAfterPickup = false;

    private const float FadeOutSeconds = 0.5f;

    private bool pickedUp;
    private Coroutine returnRoutine;

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

    private void Start()
    {
        handBopPickup = HandBopAfterItemPickup.instance;
    }

    public void Pickup()
    {
        if (pickedUp) return;
        pickedUp = true;

        handBopPickup.Do();
        transform.DOKill();
        myLight.DOKill();
        myCollider.enabled = false;
        FadeOut();
    }

    private void OnEnable()
    {
        pickedUp = false;
        returnRoutine = null;

        bool wasTaken = !myCollider.enabled;
        myCollider.enabled = true;
        rend.enabled = true;
        if (particle != null) emission.enabled = true;

        if (myLight == null) return;

        myLight.DOKill();
        myLight.intensity = wasTaken ? 0f : myLightIntestivity;
        if (wasTaken) FadeIn();
    }

    private void OnDisable()
    {
        transform.DOKill();
        if (myLight)
        {
            myLight.DOKill();
            myLight.intensity = 0f;
        }
    }

    private void FadeIn()
    {
        myLight.DOIntensity(myLightIntestivity, 0.5f).SetUpdate(true);
    }

    private void FadeOut()
    {
        rend.enabled = false;

        if (particle != null)
        {
            emission.enabled = false;
        }

        if (myLight != null) myLight.DOIntensity(0, FadeOutSeconds).SetUpdate(true);

        if (returnRoutine != null) StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(ReturnWhenFaded());
    }

    private IEnumerator ReturnWhenFaded()
    {
        yield return new WaitForSecondsRealtime(FadeOutSeconds);
        returnRoutine = null;
        DestroyMe();
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
