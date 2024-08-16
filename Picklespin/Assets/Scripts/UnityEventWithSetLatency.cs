using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventWithSetLatency : MonoBehaviour
{
    private WaitForSeconds latency;
    [SerializeField] private float setLatency = 1f;
    [SerializeField] private UnityEvent _event;


    private void Awake()
    {
        latency = new WaitForSeconds(setLatency);
    }


    public void Do()
    {
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return latency;
        _event.Invoke();
    }
}
