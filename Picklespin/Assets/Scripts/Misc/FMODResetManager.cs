using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODResetManager : MonoBehaviour
{

    public static FMODResetManager instance;

    private Bus diageticBus;

    // ~0.62s (1/φ): long enough to hide the all-emitters-start-at-once onset, short enough to feel instant
    private static readonly float fadeInDuration = 1f / PhiMath.PHI;

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

    // scene just loaded: whatever the previous scene's transition left on the bus
    // (mute, ducked tails), start from guaranteed silence and ease up to full volume
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
        // order matters: silence the bus BEFORE clearing snapshots — releasing the
        // Deathscreen duck first would pop every still-looping event back to full
        // volume for its fade-out tail (audible burst on level restart)
        diageticBus.setMute(true);
        diageticBus.stopAllEvents(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        AudioSnapshotManager.Instance.Clear();
    }

    public void MuteDiagetic()
    {
        diageticBus.setMute(true);
    }
}
