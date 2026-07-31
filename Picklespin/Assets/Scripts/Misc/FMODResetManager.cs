using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODResetManager : MonoBehaviour
{

    public static FMODResetManager instance;

    private Bus diageticBus;

    // long enough to hide the all-emitters-start-at-once onset, short enough to feel instant
    private static readonly float fadeInDuration = 0.6f;

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
        diageticBus = RuntimeManager.GetBus("bus:/diagetic_ALL");
        StartCoroutine(FadeInFromSilence());
    }

    // start from guaranteed silence, whatever the previous scene left on the bus
    private IEnumerator FadeInFromSilence()
    {
        diageticBus.setVolume(0f);
        diageticBus.setMute(false);

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            diageticBus.setVolume(Mathf.Clamp01(elapsed / fadeInDuration));
            yield return null;
        }
        diageticBus.setVolume(1f);
    }

    public void ResetFMOD(bool immediate)
    {
        // silence the bus BEFORE clearing snapshots: releasing the duck first pops
        // every still-looping event back to full volume for its fade-out tail
        diageticBus.setMute(true);
        diageticBus.stopAllEvents(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        AudioSnapshotManager.Instance.Clear();
    }

    public void MuteDiagetic()
    {
        diageticBus.setMute(true);
    }
}
