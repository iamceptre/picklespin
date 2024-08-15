using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventLatency : MonoBehaviour
{
    [SerializeField]private UnityEvent _event;

    void Start()
    {
        StartCoroutine(Do());
    }

    private IEnumerator Do()
    {
        yield return null;
        yield return null;
        _event.Invoke();
    }
}
