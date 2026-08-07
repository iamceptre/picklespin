using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

// The only way out of a scene. Every exit does the same three things first, which is what
// keeps a restart from carrying tweens, a frozen clock or a half-stopped mix into the next scene.
public static class SceneFlow
{
    public static bool IsLeaving { get; private set; }

    public static void Reload() => Load(SceneManager.GetActiveScene().buildIndex);

    public static void Load(int buildIndex)
    {
        if (!BeginLeaving(true)) return;

        SceneManager.LoadScene(buildIndex);
    }

    // for a loading bar: the current scene stays up while the next one streams in, so its
    // audio is faded out rather than cut. Activate the returned operation when the bar is done.
    public static AsyncOperation LoadAsync(int buildIndex)
    {
        if (!BeginLeaving(false)) return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex);
        operation.allowSceneActivation = false;
        return operation;
    }

    public static void Quit()
    {
        if (!BeginLeaving(true)) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static bool BeginLeaving(bool cutAudioNow)
    {
        if (IsLeaving) return false;
        IsLeaving = true;

        Time.timeScale = 1f;
        DOTween.KillAll();
        AudioTransition.Silence(cutAudioNow);
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        IsLeaving = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsLeaving = false;

        // asking for a scene change does not stop the frame it was asked in: whatever ticks
        // after the kill can still start a tween, and by its first update the unload has taken
        // the target away. This drops exactly those, and leaves the new scene's own tweens alone
        DOTween.Validate();

        // a scene with a reset manager of its own is already holding the bus at silence to fade
        // up out of it; lifting the mute here is for the scenes that have none
        if (!FMODResetManager.instance) AudioTransition.Restore();
    }
}
