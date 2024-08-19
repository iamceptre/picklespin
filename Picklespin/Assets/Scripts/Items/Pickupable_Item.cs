using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Pickupable_Item : MonoBehaviour
{

    [SerializeField] private UnityEvent pickupEvent;

    private void Start()
    {
        StartFloating();
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
        transform.DOMoveY(transform.position.y + 0.3f, 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * 30 , Space.Self);
    }


}
