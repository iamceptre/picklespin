using FMODUnity;
using UnityEngine;

public class AmbianceManager : MonoBehaviour
{

    [SerializeField] private StudioEventEmitter[] dynamicBgEmitter;
    [SerializeField] private StudioEventEmitter[] staticBgEmitter;

    [SerializeField] private bool playAmbianceSetOnStart = true;
    [SerializeField] private int startingAmbiance = 0;

    private void Start()
    {
        StartingAmbiance(startingAmbiance);
    }

    private void StartingAmbiance(int index)
    {
        PlaySelectedDynamic(index);
        PlaySelectedStatic(index);
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
    }

    public void PlaySelectedDynamic(int index)
    {
        StopAllDynamic();
        dynamicBgEmitter[index].Play();
    }

    public void PlaySelectedStatic(int index)
    {
        StopAllStatic();
        staticBgEmitter[index].Play();
    }

    public void StopAllStatic()
    {
        for (int i = 0; i < dynamicBgEmitter.Length; i++)
        {
            staticBgEmitter[i].Stop();
        }
    }

    public void StopSelectedStatic(int index)
    {
        staticBgEmitter[index].Stop();
    }
}
