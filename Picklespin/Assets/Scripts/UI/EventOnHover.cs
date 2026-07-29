using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EventOnHover : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;

    [SerializeField] private UnityEvent hoverEvent;
    [SerializeField] private UnityEvent exitEvent;

    private bool hovered = false;

    private void Awake()
    {
        if (!rectTransform)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        if (!canvas)
        {
            canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        }
    }

    private void Update()
    {
        bool hovering = IsPointerOverRect();

        if (hovering && !hovered)
        {
            hovered = true;
            hoverEvent.Invoke();
        }
        else if (!hovering && hovered)
        {
            hovered = false;
            exitEvent.Invoke();
        }
    }

    // Polled every frame instead of relying on OnPointerEnter/OnPointerExit,
    // which can lose their pairing (stuck-visible tap controls) when the
    // pointer crosses several UI elements in a single fast movement.
    private bool IsPointerOverRect()
    {
        if (Mouse.current == null)
        {
            return false;
        }

        Vector2 pointerPos = Mouse.current.position.ReadValue();
        Camera eventCamera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pointerPos, eventCamera);
    }
}
