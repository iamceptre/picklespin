using System.Collections;
using FMOD.Studio;
using UnityEngine;

// Every scene that has a world starts from guaranteed silence, whatever the previous one left
// on the bus, and rises out of it. Leaving a scene is SceneFlow's job, not this one's.
public class FMODResetManager : MonoBehaviour
{
    public static FMODResetManager instance;

    // long enough to hide the all-emitters-start-at-once onset, short enough to feel instant
    private const float fadeInDuration = 0.6f;

    private const float waitForBusTimeout = 5f;

    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;

        AudioTransition.SetDiageticVolume(0f);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Start() => StartCoroutine(FadeInFromSilence());

    private IEnumerator FadeInFromSilence()
    {
        // in a build the banks can still be loading, and a bus that is not there yet would
        // leave the mix wherever the last scene left it
        float waited = 0f;
        while (!AudioTransition.TryGetDiageticBus(out Bus _))
        {
            waited += Time.unscaledDeltaTime;
            if (waited > waitForBusTimeout)
            {
                DevLog.Error($"{nameof(FMODResetManager)}: '{AudioTransition.DiageticBusPath}' never showed up - the mix stays as the last scene left it", this);
                yield break;
            }
            yield return null;
        }

        AudioTransition.SetDiageticVolume(0f);

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            AudioTransition.SetDiageticVolume(Mathf.Clamp01(elapsed * (1f / fadeInDuration)));
            yield return null;
        }

        AudioTransition.SetDiageticVolume(1f);
    }
}
