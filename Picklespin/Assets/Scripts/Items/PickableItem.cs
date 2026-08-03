using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using FMODUnity;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PickableItem : MonoBehaviour
{

    [SerializeField] private UnityEvent pickupEvent;
    [SerializeField] private StudioEventEmitter spawnSoundEmitter;

    private const float FadeSeconds = 0.5f;

    private HandBopAfterItemPickup handBopPickup;
    private PoolSpawnableObject pooled;

    private Collider myCollider;
    private Renderer rend;
    private Light myLight;
    private float myLightIntensity;
    private ParticleSystem particle;
    private ParticleSystem.EmissionModule emission;

    private bool pickedUp;
    private Coroutine returnRoutine;

    private void Awake()
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

        pooled = GetComponent<PoolSpawnableObject>();

        if (myLight != null)
        {
            myLightIntensity = myLight.intensity;
        }
    }

    private HandBopAfterItemPickup HandBop =>
        handBopPickup ? handBopPickup : handBopPickup = HandBopAfterItemPickup.instance;

    private void Start()
    {
        handBopPickup = HandBopAfterItemPickup.instance;
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
        myLight.intensity = wasTaken ? 0f : myLightIntensity;
        if (wasTaken) myLight.DOIntensity(myLightIntensity, FadeSeconds).SetUpdate(true);
    }

    private void OnDisable()
    {
        if (gameObject.IsUnloading()) return;

        transform.DOKill();
        if (myLight)
        {
            myLight.DOKill();
            myLight.intensity = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            pickupEvent.Invoke();
        }
    }

    public void StartFloating()
    {
        if (spawnSoundEmitter) spawnSoundEmitter.Play();

        transform.DOMoveY(transform.position.y + 0.3f, 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * 30 , Space.Self);
    }

    public void Pickup()
    {
        if (pickedUp) return;
        pickedUp = true;

        if (HandBop) HandBop.Do();
        transform.DOKill();
        if (myLight) myLight.DOKill();
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

        if (myLight != null) myLight.DOIntensity(0, FadeSeconds).SetUpdate(true);

        if (returnRoutine != null) StopCoroutine(returnRoutine);
        returnRoutine = StartCoroutine(ReturnWhenFaded());
    }

    private IEnumerator ReturnWhenFaded()
    {
        yield return new WaitForSecondsRealtime(FadeSeconds);
        returnRoutine = null;

        if (pooled) pooled.FreeUpSlot();
        else Destroy(gameObject);
    }

}
