using UnityEngine;
using FMODUnity;

public class SnapshotManager : MonoBehaviour
{

    public static SnapshotManager instance;

    public StudioEventEmitter LowHp;
    public StudioEventEmitter PortalClosed;
    public StudioEventEmitter Pause;
    public StudioEventEmitter Deathscreen;

    [Header("Reverbs")]
    [SerializeField] private StudioEventEmitter churchReverb;
    [SerializeField] private StudioEventEmitter corridorReverb;
    [SerializeField] private StudioEventEmitter angelReverb;

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
       // StopAllSnapshots(); //nonsense, beacuse when loading a new scene, the reference is lost
        PlayReverbSnapshot(0);
    }


    public void PlayReverbSnapshot(int index) //USE STOP ALL BEFORE PLAYING INSTEAD TO AVOID SPAGHETII
    {
        switch (index)
        {
            case 0: //church
                angelReverb.Stop();
                corridorReverb.Stop();
                churchReverb.Play();
                break;

            case 1: //corridor
                angelReverb.Stop();
                churchReverb.Stop();
                corridorReverb.Play();
                break;

            case 2: //angel
                churchReverb.Stop();
                corridorReverb.Stop();
                angelReverb.Play();
                break;

            default:
                break;
        }

    }

    public void StopReverbSnapshot(int index)
    {
        switch (index)
        {
            case 0: //church
                churchReverb.Stop();
                break;

            case 1: //corridor
                corridorReverb.Stop();
                break;

            case 2: //angel
                angelReverb.Stop();
                break;

            default:
                break;
        }
    }


    public void StopAllSnapshots() {
        LowHp.Stop();
        PortalClosed.Stop();
        Pause.Stop();
        Deathscreen.Stop();
        StopReverbSnapshot(0);
        StopReverbSnapshot(1);
        StopReverbSnapshot(2);
        StopReverbSnapshot(3);
    }

}
