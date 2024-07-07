using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class SnapshotManager : MonoBehaviour
{

    public static SnapshotManager instance;

    private EventInstance lowHpSnapshotInstance;
    [SerializeField] private EventReference lowHpSnapshotReference;

    private EventInstance portalClosedSnapshotInstance;
    [SerializeField] private EventReference portalClosedSnapshotReference;

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
    }



}
