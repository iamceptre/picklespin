using UnityEngine;

public class ActivateSelectedReverbOnTriggerEnter : MonoBehaviour
{
    [Header("Select only one box!")]

    [SerializeField] private bool churchReverbMainHall; //index 0
    [SerializeField] private bool corridorReverb; //index 1
    [SerializeField] private bool angelRoomReverb; //index 2
    
    private SnapshotManager snapshotManager;
    private AmbianceManager ambianceManager;

    private int reverbIndex;

    private void Start()
    {
        snapshotManager = SnapshotManager.instance;
        ambianceManager = AmbianceManager.instance;

        if (churchReverbMainHall)
        {
            reverbIndex = 0; //church
            return;
        }

        if (corridorReverb)
        {
            reverbIndex = 1; //dungeon
            return;
        }

        if (angelRoomReverb)
        {
            reverbIndex = 2; //angelroom
            return;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (ambianceManager.cachedIndex != reverbIndex)
            {
                snapshotManager.PlayReverbSnapshot(reverbIndex);
                ambianceManager.PlaySelectedSet(reverbIndex);
            }
        }
    }


}
