using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EventOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private UnityEvent hoverEvent;
    [SerializeField] private UnityEvent exitEvent;


    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverEvent.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        exitEvent.Invoke();
    }
}
