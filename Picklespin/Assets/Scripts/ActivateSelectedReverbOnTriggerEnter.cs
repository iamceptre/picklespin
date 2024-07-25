using UnityEngine;

public class ActivateSelectedReverbOnTriggerEnter : MonoBehaviour
{
    [Header("Select only one box!")]

    [SerializeField] private bool churchReverbMainHall; //index 0
    [SerializeField] private bool corridorReverb; //index 1
    [SerializeField] private bool angelRoomReverb; //index 2

    private SnapshotManager snapshotManager;

    private int index;

    private void Start()
    {
        snapshotManager = SnapshotManager.instance;

        if (churchReverbMainHall)
        {
            index = 0;
            return;
        }

        if (corridorReverb)
        {
            index = 1;
            return;
        }

        if (angelRoomReverb)
        {
            index = 2;
            return;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            snapshotManager.PlayReverbSnapshot(index);
        }
    }


}
