using UnityEngine;
using UnityEngine.Events;

public class EventOnEnable : MonoBehaviour
{
    [SerializeField] private UnityEvent _event;
    private void OnEnable()
    {
        _event.Invoke();
    }
}
