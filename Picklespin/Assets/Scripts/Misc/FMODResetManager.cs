using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODResetManager : MonoBehaviour
{

    public static FMODResetManager instance;

    private Bus MasterBus;
    private Bus diageticBus;

    private SnapshotManager snapshotManager;

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
        snapshotManager = SnapshotManager.instance;
        //MasterBus = RuntimeManager.GetBus("bus:/");
        MasterBus = RuntimeManager.GetBus("bus:/diagetic_ALL");
        diageticBus = RuntimeManager.GetBus("bus:/diagetic_ALL");
    }
    public void ResetFMOD(bool immediate)
    {
        //Debug.Log("reset fmod");
        if(snapshotManager != null)
        snapshotManager.StopAllSnapshots();

        if (immediate)
        {
            MasterBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        else
        {
            MasterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        diageticBus.setMute(false);

    }

    public void MuteDiagetic()
    {
        diageticBus.setMute(true);
    }
}