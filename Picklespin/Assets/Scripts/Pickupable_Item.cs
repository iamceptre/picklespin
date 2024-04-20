using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Pickupable_Item : MonoBehaviour
{

    [SerializeField] private UnityEvent pickupEvent;
    private Light myLight;
    private float startingYPos;
    private float myLightIntensitivity;

    void Awake()
    {
        myLight = GetComponent<Light>();

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
            pickupEvent.Invoke();
        }
    }

    private void PickupableAnimation()
    {
        transform.DOMoveY(startingYPos + 0.3f, 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        if (myLight != null) {
            myLight.DOIntensity(myLightIntensitivity * 0.5f, 1).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * 30 , Space.Self);
    }


}
