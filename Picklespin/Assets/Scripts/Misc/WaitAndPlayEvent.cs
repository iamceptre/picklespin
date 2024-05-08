using UnityEngine;
using FMODUnity;

public class WaitAndPlayEvent : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter emitter;

    [SerializeField] private bool startOnStart;

    [SerializeField] private float waitTime;
    [Header("Random")]
    [SerializeField] private bool randomize;
    [SerializeField] private float randomMinTime;
    [SerializeField] private float randomMaxTime;

    void Start()
    {

        if (emitter == null)
        {
            emitter = gameObject.GetComponent<StudioEventEmitter>(); 
        }

        if (startOnStart)
        {
            Do();
        }
    }


    public void Do()
    {

        if (!randomize)
        {
            Invoke("PlayEvent", waitTime);
        }
        else
        {
            Invoke("PlayEvent", Random.Range(randomMinTime, randomMaxTime));
        }
    }

    private void PlayEvent()
    {
        emitter.Play(); 
    }

}
