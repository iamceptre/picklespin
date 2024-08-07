using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;

public class UIHoverCheck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    [SerializeField] private UnityEvent hoverEvent;
    [SerializeField] private UnityEvent unhoverEvent;

    private bool hovering = false;
    private bool hovered = false;
    private bool externalHover = false;

    private void Awake()
    {
        if (TryGetComponent(out GraphicRaycaster rc))
        {
            raycaster = rc;
        }


        eventSystem = EventSystem.current;
    }

    public void SetExternalHover(bool state)
    {
        externalHover = state;
    }

    private void Update()
    {
        if (hovering || externalHover)
        {
            if (!hovered)
            {
                hovered = true;
                hoverEvent.Invoke();
            }

        }
        else
        {
            if (hovered)
            {
                hovered = false;
                unhoverEvent.Invoke();
            }

        }
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}