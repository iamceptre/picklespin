using UnityEngine;
using UnityEngine.Events;

public class ActivateSelectedReverbOnTriggerEnter : MonoBehaviour
{
    [Header("Select only one box!")]

    [SerializeField] private bool churchReverbMainHall; //index 0
    [SerializeField] private bool corridorReverb; //index 1
    [SerializeField] private bool angelRoomReverb; //index 2


    [SerializeField] private UnityEvent onEnterEvent;

    [Header("Ambiance Events: ")]
    //add some crazy ambiance events, make then so that by playing just one ambiance from unity, the whole ambience changes (both dynamic and static (nested events ig))

    private SnapshotManager snapshotManager;

    private int reverbIndex;

    private void Start()
    {
        snapshotManager = SnapshotManager.instance;

        if (churchReverbMainHall)
        {
            reverbIndex = 0;
            return;
        }

        if (corridorReverb)
        {
            reverbIndex = 1;
            return;
        }

        if (angelRoomReverb)
        {
            reverbIndex = 2;
            return;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            snapshotManager.PlayReverbSnapshot(reverbIndex);
            onEnterEvent.Invoke();
        }
    }


}
