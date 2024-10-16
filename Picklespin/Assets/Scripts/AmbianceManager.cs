using FMODUnity;
using UnityEngine;

public class AmbianceManager : MonoBehaviour
{

    public static AmbianceManager instance { get; private set; }

    [SerializeField] private StudioEventEmitter[] dynamicBgEmitter;
    [SerializeField] private StudioEventEmitter[] staticBgEmitter;

    //[SerializeField] private bool playAmbianceSetOnStart = true;
    [SerializeField] private int startingAmbiance = 0;

    public int cachedIndex = -1;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

    }

    private void Start()
    {
        StartingAmbiance(startingAmbiance);
    }

    private void StartingAmbiance(int index)
    {
        dynamicBgEmitter[index].Play();
        staticBgEmitter[index].Play();
        cachedIndex = index;
    }

    public void StopAllDynamic()
    {
        for (int i = 0; i < dynamicBgEmitter.Length; i++)
        {
            dynamicBgEmitter[i].Stop();
        }
    }
    public void StopSelectedDynamic(int index)
    {
        dynamicBgEmitter[index].Stop();
        cachedIndex = index;
    }

    public void PlaySelectedDynamic(int index)
    {
        if (index != cachedIndex)
        {
            StopAllDynamic();
            dynamicBgEmitter[index].Play();
        }
        cachedIndex = index;
    }

    public void PlaySelectedSet(int index)
    {
        if (index != cachedIndex)
        {
            StopAllDynamic();
            StopAllStatic();

            dynamicBgEmitter[index].Play();

            if (index != 0) //NEVER PLAYS THE MAIN CHURCH WIND AGAIN
            {
                staticBgEmitter[index].Play();
            }
        }

        cachedIndex = index;

    }

    public void PlaySelectedStatic(int index)
    {
        if (index != cachedIndex)
        {
            StopAllStatic();
            staticBgEmitter[index].Play();
        }
        cachedIndex = index;
    }

    public void StopAllStatic()
    {
        for (int i = 0; i < dynamicBgEmitter.Length; i++)
        {
            if (i != 0)
            { //NEVERS STOPS THE CHURCH MAIN WIND
                staticBgEmitter[i].Stop();
            }
        }
    }

    public void StopSelectedStatic(int index)
    {
        staticBgEmitter[index].Stop();
        cachedIndex = index;
    }

}
