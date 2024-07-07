using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODResetManager : MonoBehaviour
{

    public static FMODResetManager instance;

    private Bus MasterBus;

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
        MasterBus = RuntimeManager.GetBus("bus:/");
    }
    public void ResetFMOD(bool immediate)
    {
        //Debug.Log("reset fmod");

        snapshotManager.StopAllSnapshots();

        if (immediate)
        {
            MasterBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        else
        {
            MasterBus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

    }
}