using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System;

public class SnapshotManager : MonoBehaviour
{

    public static SnapshotManager instance;

    private EventInstance lowHpSnapshotInstance;
    [SerializeField] private EventReference lowHpSnapshotReference;

    private EventInstance portalClosedSnapshotInstance;
    [SerializeField] private EventReference portalClosedSnapshotReference;

    [Header("Reverbs")]
    [SerializeField] private StudioEventEmitter churchReverb;
    [SerializeField] private StudioEventEmitter corridorReverb;

    //THIS SCRIPT IS FUCKING STUPID CAVEMAN SHIT
    //FUCKING HATE THIS FUCKING HANGOVER FUCK
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

        lowHpSnapshotInstance = RuntimeManager.CreateInstance(lowHpSnapshotReference);
        portalClosedSnapshotInstance = RuntimeManager.CreateInstance(portalClosedSnapshotReference);
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
                Debug.Log("there is no angelroom stop function");
                break;

            default:
                break;
        }
    }



    public void PlayLowHPSnapshot()
    {
        lowHpSnapshotInstance.start();
    }

    public void StopLowHPSnapshot()
    {
        lowHpSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PlayPortalClosedSnapshot()
    {
        portalClosedSnapshotInstance.start();
    }

    public void StopPortalClosedSnapshot()
    {
        portalClosedSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }


    public void StopAllSnapshots() {
        //LOW HP
        lowHpSnapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        lowHpSnapshotInstance.release();

        //PORTAL CLOSED
        portalClosedSnapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        portalClosedSnapshotInstance.release();

        StopReverbSnapshot(0);
        StopReverbSnapshot(1);
        StopReverbSnapshot(2);
    }



}
