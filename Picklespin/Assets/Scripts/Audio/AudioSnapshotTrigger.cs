using UnityEngine;

public class AudioSnapshotTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] [Tooltip("ex. Reverb Snapshot is exclusive")] private bool isExclusive = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isExclusive)
            {
                AudioSnapshotManager.Instance.SwitchExclusiveSnapshot(sceneName);
            }
            else
            {
                AudioSnapshotManager.Instance.EnableSnapshot(sceneName);
            }
        }
    }
}
