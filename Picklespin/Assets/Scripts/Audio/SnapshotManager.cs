using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class SnapshotManager : MonoBehaviour
{

    public static SnapshotManager instance;

    public StudioEventEmitter LowHp;
    public StudioEventEmitter PortalClosed;
    public StudioEventEmitter Pause;

    [Header("Reverbs")]
    [SerializeField] private StudioEventEmitter churchReverb;
    [SerializeField] private StudioEventEmitter corridorReverb;

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
        PlayReverbSnapshot(0);
    }


    public void PlayReverbSnapshot(int index)
    {
        switch (index)
        {
            case 0: //church
                corridorReverb.Stop();
                churchReverb.Play();
                break;

            case 1: //corridor
                churchReverb.Stop();
                corridorReverb.Play();
                break;

            case 2: //angel
                churchReverb.Stop();
                corridorReverb.Stop();
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
                Debug.Log("there is no angelroom reverb stop function");
                break;

            default:
                break;
        }
    }


    public void StopAllSnapshots() {
        LowHp.Stop();
        PortalClosed.Stop();

        StopReverbSnapshot(0);
        StopReverbSnapshot(1);
        StopReverbSnapshot(2);
    }



}
