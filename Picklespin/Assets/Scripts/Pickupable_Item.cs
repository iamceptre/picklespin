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
        transform.DORotate(new Vector3(360, 360, 0), 1).SetLoops(-1);
        myLight.DOIntensity(myLightIntensitivity * 0.5f, 1).SetLoops(-1, LoopType.Yoyo);
    }


}
