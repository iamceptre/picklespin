using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioSnapshotManager : MonoBehaviour
{
    public static AudioSnapshotManager Instance { get; private set; }

    [System.Serializable]
    public class SnapshotData
    {
        [Tooltip("Unique key to reference this snapshot in code.")]
        public string key;

        [Tooltip("FMOD snapshot event.")]
        public EventReference snapshot;

        [Tooltip("If true, only one snapshot of this type can be active at a time.")]
        public bool isExclusive;
    }

    [Header("All Snapshots")]
    [SerializeField]
    private List<SnapshotData> snapshots = new();

    // Dictionary for quick lookups instead of using snapshots.Find(...)
    private Dictionary<string, SnapshotData> snapshotDictionary;

    private readonly Dictionary<string, EventInstance> activeSnapshots = new();
    private string activeExclusive = "";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build a dictionary for O(1) lookups.
        snapshotDictionary = new Dictionary<string, SnapshotData>(snapshots.Count);
        foreach (SnapshotData data in snapshots)
        {
            if (!snapshotDictionary.ContainsKey(data.key))
            {
                snapshotDictionary.Add(data.key, data);
            }
            else
            {
                Debug.LogWarning($"Duplicate snapshot key found: {data.key}");
            }
        }
    }

    public void EnableSnapshot(string key)
    {
        // Debug.Log($"Enabling snapshot: {key}");

        if (!snapshotDictionary.TryGetValue(key, out SnapshotData data))
        {
            Debug.LogWarning($"Snapshot '{key}' not found in dictionary!");
            return;
        }

        if (data.isExclusive)
        {
            if (key == activeExclusive)
            {
                return;
            }

            if (!string.IsNullOrEmpty(activeExclusive))
            {
                DisableSnapshot(activeExclusive);
            }

            activeExclusive = key;
        }

        if (!activeSnapshots.ContainsKey(key))
        {
            EventInstance instance = RuntimeManager.CreateInstance(data.snapshot);
            instance.start();
            instance.release();
            activeSnapshots[key] = instance;
        }
    }

    public void DisableSnapshot(string key)
    {
        // Debug.Log($"Disabling snapshot: {key}");
        if (activeSnapshots.TryGetValue(key, out EventInstance instance))
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            activeSnapshots.Remove(key);

            if (snapshotDictionary.TryGetValue(key, out SnapshotData data))
            {
                if (data.isExclusive && activeExclusive == key)
                {
                    activeExclusive = "";
                }
            }
        }
    }

    public void SwitchExclusiveSnapshot(string key)
    {
        // Debug.Log($"Switching to exclusive snapshot: {key}");
        if (!snapshotDictionary.TryGetValue(key, out SnapshotData data) || !data.isExclusive)
        {
            Debug.LogWarning($"Exclusive snapshot '{key}' not found or not marked exclusive!");
            return;
        }

        if (key == activeExclusive)
        {
            return;
        }

        if (!string.IsNullOrEmpty(activeExclusive))
        {
            DisableSnapshot(activeExclusive);
        }

        EnableSnapshot(key);
    }

    private void OnDestroy()
    {
        Clear();
    }

    public void Clear()
    {
        foreach (EventInstance snapshot in activeSnapshots.Values)
        {
            snapshot.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        activeSnapshots.Clear();
        activeExclusive = "";
    }
}
